﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Linq;
using Microsoft.Win32;
using System.Diagnostics;

namespace AngelSix.SolidWorksApi.AddinInstaller
{
    /// <summary>
    /// A tool to aid in running the correct RegAsm command on a SolidWorks Add-in Dll
    /// to visually install and uninstall add-ins
    /// 
    /// NOTE: This application is designed to run on x64 machines and x64 installs of SolidWorks by default
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Members

        /// <summary>
        /// The name of the SolidWorks exe
        /// </summary>
        private string mSolidWorksFilename = "SLDWORKS.exe";

        /// <summary>
        /// The name of the RegAsm tool
        /// </summary>
        private string mRegAsmFilename = "RegAsm.exe";

        /// <summary>
        /// The folder location where 64bit .Net Frameworks are installed, relative to the Windows folder
        /// </summary>
        private string mRegAsmWindowsPath = "Microsoft.NET\\Framework64";

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Try and locate SolidWorks
            LocateSolidWorks();

            // Try and local RegAsm
            LocateRegAsm();
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Checks we have a SolidWorks application, regasm and an add-in
        /// </summary>
        /// <returns></returns>
        private bool SanityCheck()
        {
            // Check SolidWorks
            if (string.IsNullOrEmpty(SolidWorksPath.Text) || !File.Exists(SolidWorksPath.Text))
            {
                MessageBox.Show("Please specify a path to a valid SolidWorks application", "No SolidWorks found");
                return false;
            }

            // Check RegAsm
            if (string.IsNullOrEmpty(RegAsmPath.Text) || !File.Exists(RegAsmPath.Text))
            {
                MessageBox.Show("Please specify a path to a valid RegAsm application", "No RegAsm found");
                return false;
            }

            // Check Dll
            if (string.IsNullOrEmpty(DllPath.Text) || !File.Exists(DllPath.Text))
            {
                MessageBox.Show("Please specify a path to a valid SolidWorks Add-in dll", "No Add-in found");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Attempts to local the SolidWorks exe and set it to the path
        /// </summary>
        private void LocateSolidWorks()
        {
            // Locate SolidWorks exe in Program Files
            var results = new List<string>();
            FindByFilename(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "SolidWorks", mSolidWorksFilename, results);

            // If we have at least one SolidWorks, use that
            if (results?.Count > 0)
                SolidWorksPath.Text = results.First();
        }

        /// <summary>
        /// Attempts to local the RegAsm exe and set it to the path
        /// </summary>
        private void LocateRegAsm()
        {
            // Locate SolidWorks exe in Program Files
            var results = new List<string>();
            FindByFilename(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), mRegAsmWindowsPath), null, mRegAsmFilename, results);

            // If we have at least one, use the last one (so newest version)
            if (results?.Count > 0)
                RegAsmPath.Text = results.Last();
        }

        /// <summary>
        /// Gets all files that match the search pattern
        /// Searches recursively
        /// </summary>
        /// <param name="path">The path to search within</param>
        /// <param name="pathContains">The string the path must contain in order to process further down the tree, or null to check all</param>
        /// <param name="filename">The filename to find (case insensitive)</param>
        /// <param name="results">The results to store the results in</param>
        /// <returns></returns>
        private void FindByFilename(string path, string pathContains, string filename, List<string> results = null)
        {
            // Create new list if none passed in
            if (results == null)
                results = new List<string>();

            // Get all files in the current folder
            try
            {
                var files = Directory.EnumerateFiles(path).Where(f => string.Equals(Path.GetFileName(f), filename, StringComparison.InvariantCultureIgnoreCase)).ToList();
                if (files?.Count > 0)
                    results.AddRange(files);
            }
            catch
            { }

            // Search all sub-folders
            try
            {
                // Allow case-insensitive checking
                if (pathContains != null)
                    pathContains = pathContains.ToLower();

                // Search into directories that match
                Directory.EnumerateDirectories(path).Where(f => string.IsNullOrEmpty(pathContains) || f.ToLower().Contains(pathContains)).ToList().ForEach(dir => FindByFilename(dir, null, filename, results));
            }
            catch { }
        }

        #endregion

        #region UI Events

        /// <summary>
        /// Browse for a SolidWorks exe manually
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseSolidWorksButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = $"SolidWorks | {mSolidWorksFilename}",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
            };

            // Open dialog
            var result = ofd.ShowDialog();

            // If they cancelled, return
            if (!result.HasValue || !result.Value)
                return;

            // If they selected a file, use that
            SolidWorksPath.Text = ofd.FileName;
        }

        /// <summary>
        /// Browse for RegAsm manually
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseRegAsmButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = $"RegAsm | {mRegAsmFilename}",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows)
            };

            // Open dialog
            var result = ofd.ShowDialog();

            // If they cancelled, return
            if (!result.HasValue || !result.Value)
                return;

            // If they selected a file, use that
            RegAsmPath.Text = ofd.FileName;
        }

        /// <summary>
        /// Browse for an add-in dll
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseDllButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = $"Add-in Dll | *.dll",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            // Open dialog
            var result = ofd.ShowDialog();

            // If they cancelled, return
            if (!result.HasValue || !result.Value)
                return;

            // If they selected a file, use that
            DllPath.Text = ofd.FileName;
        }

        /// <summary>
        /// Installs the selected add-in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            // Sanity check
            if (!SanityCheck())
                return;

            // Run the RegAsm with the Dll path as an argument
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = RegAsmPath.Text,
                    Arguments = $"/codebase \"{DllPath.Text}\"",
                    // Run as admin
                    Verb = "runas",
                    // Redirect input and output
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            
            // Read the output
            var result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // If the last bit of text is "Types registered successfully, show success
            if (result.Replace("\r", "").Replace("\n", "").EndsWith("Types registered successfully"))
                MessageBox.Show("Add-in was successfully registered", "Success");
            // Otherwise just show the results
            else
                MessageBox.Show(result, "Unexpected Response", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// Uninstalls the selected add-in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UninstallButton_Click(object sender, RoutedEventArgs e)
        {
            // Sanity check
            if (!SanityCheck())
                return;

            // Run the RegAsm with the Dll path as an argument
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = RegAsmPath.Text,
                    Arguments = $"/u \"{DllPath.Text}\"",
                    // Run as admin
                    Verb = "runas",
                    // Redirect input and output
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            // Read the output
            var result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // If the last bit of text is "Types registered successfully, show success
            if (result.Replace("\r", "").Replace("\n", "").EndsWith("Types un-registered successfully"))
                MessageBox.Show("Add-in was successfully un-registered", "Success");
            // Otherwise just show the results
            else
                MessageBox.Show(result, "Unexpected Response", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        #endregion
    }
}
