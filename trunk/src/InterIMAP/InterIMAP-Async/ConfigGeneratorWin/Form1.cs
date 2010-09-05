using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using InterIMAP;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using InterIMAP.Synchronous;

namespace ConfigGeneratorWin
{
    public partial class Form1 : Form
    {
        #region CTOR
        public Form1()
        {
            InitializeComponent();

            config = new IMAPConfig();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            configFile = String.Empty;

            ToggleEnabled(false);
        }
        #endregion

        #region Private Fields
        private IMAPConfig config;
        private string configFile;
        private bool configChanged;
        #endregion

        #region Event Handlers
        /// <summary>
        /// Prompts the user to enter the filename of the new settings file to create
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog diag = new SaveFileDialog();
            diag.OverwritePrompt = true;
            if (diag.ShowDialog() == DialogResult.OK)
            {
                configFile = diag.FileName;
                config = new IMAPConfig();
                config.SaveConfig(configFile);
                BindConfigInfo();
                UpdateTitle();
                configChanged = false;
            }

            ToggleEnabled(true);
        }

        /// <summary>
        /// Prompts user to select an existing config file, and then opens it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog diag = new OpenFileDialog();
            diag.Multiselect = false;

            if (diag.ShowDialog() == DialogResult.OK)
            {
                
                configFile = diag.FileName;
                Stream s = File.Open(configFile, FileMode.Open);
                BinaryFormatter b = new BinaryFormatter();
                config = (IMAPConfig)b.Deserialize(s);
                

                s.Close();

                BindConfigInfo();
                UpdateTitle();
                configChanged = false;
            }

            ToggleEnabled(true);
            
        }

        /// <summary>
        /// Prompts user to specify the name of the file where the local cache will be stored
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void browseButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog diag = new SaveFileDialog();
            if (diag.ShowDialog() == DialogResult.OK)
            {
                config.CacheFile = diag.FileName;
                localCacheBox.Text = diag.FileName;
            }
        }

        private void logFileBrowse_Click(object sender, EventArgs e)
        {
            SaveFileDialog diag = new SaveFileDialog();
            if (diag.ShowDialog() == DialogResult.OK)
            {
                config.LogFile = diag.FileName;
                logFileBox.Text = diag.FileName;
            }
        }

        /// <summary>
        /// Saves the config settings to the current file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveBtn_Click(object sender, EventArgs e)
        {
            if (passwordBox.Text != String.Empty)
            {
                if (passwordBox2.Text == String.Empty)
                {
                    MessageBox.Show("You must verify your password before you can save.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!passwordBox.Text.Equals(passwordBox2.Text))
                {
                    MessageBox.Show("Passwords do not match", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            if (debugMode.Checked && debugDetailBox.SelectedIndex < 1)
            {
                MessageBox.Show("Please select a detail level for the debug mode.", "Select Detail Level", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            config.SaveConfig(configFile);
            MessageBox.Show("Settings saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            configChanged = false;
        }

        /// <summary>
        /// Closes the application, prompting the user to save changes if any values have been changed since the last save
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeBtn_Click(object sender, EventArgs e)
        {
            if (configChanged)
            {
                DialogResult res = MessageBox.Show("The current configuration has been changed, would you like to save it?", "Save Changes?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                switch (res)
                {
                    case DialogResult.Yes:
                        {
                            config.SaveConfig(configFile);
                            break;
                        }
                    case DialogResult.No:
                        {
                            
                            break;
                        }
                    case DialogResult.Cancel:
                        {

                            return;
                        }
                }
            }
            this.Close();
        }
        #endregion

        #region Helper Method
        /// <summary>
        /// Update the title bar to reflect the currently open settings file
        /// </summary>
        public void UpdateTitle()
        {
            this.Text = String.Format("InterIMAP Configuration Editor - {0}", configFile);

        }

        /// <summary>
        /// Sets the state of the controls. Used to turn the controls on once a config file is specified
        /// </summary>
        /// <param name="state"></param>
        public void ToggleEnabled(bool state)
        {
            hostName.Enabled = state;
            usernameBox.Enabled = state;
            passwordBox.Enabled = state;
            passwordBox2.Enabled = state;
            rootFolderBox.Enabled = state;
            localCacheBox.Enabled = state;
            browseButton.Enabled = state;
            xmlFormat.Enabled = state;
            binaryFormat.Enabled = state;
            useSSL.Enabled = state;
            autoLogon.Enabled = state;
            autoLoad.Enabled = state;
            debugMode.Enabled = state;
            saveBtn.Enabled = state;
            logFileBox.Enabled = state;
            logFileBrowse.Enabled = state;
            debugDetailBox.Enabled = state;
            autoSyncCacheBox.Enabled = state;
        }
        #endregion

        #region Bind Methods
        /// <summary>
        /// Updates the values of the controls with the currently loaded config object
        /// </summary>
        public void BindConfigInfo()
        {
            hostName.Text = config.Host;
            usernameBox.Text = config.UserName;
            passwordBox.Text = config.Password;
            passwordBox2.Text = config.Password;
            rootFolderBox.Text = config.DefaultFolderName;
            localCacheBox.Text = config.CacheFile;
            logFileBox.Text = config.LogFile;

            switch (config.DebugDetail)
            {
                case IMAPConfig.DebugDetailLevel.All:
                    {
                        debugDetailBox.SelectedIndex = 1;
                        break;
                    }
                case IMAPConfig.DebugDetailLevel.ErrorsOnly:
                    {
                        debugDetailBox.SelectedIndex = 3;
                        break;
                    }
                case IMAPConfig.DebugDetailLevel.InterIMAPOnly:
                    {
                        debugDetailBox.SelectedIndex = 2;
                        break;
                    }
            }

            switch (config.Format)
            {
                case CacheFormat.Binary:
                    {
                        binaryFormat.Checked = true;
                        break;
                    }
                case CacheFormat.XML:
                    {
                        xmlFormat.Checked = true;
                        break;
                    }
            }

            useSSL.Checked = config.UseSSL;
            autoLogon.Checked = config.AutoLogon;
            autoLoad.Checked = config.AutoGetMsgID;
            debugMode.Checked = config.DebugMode;
            autoSyncCacheBox.Checked = config.AutoSyncCache;
            configChanged = false;
 
        }
        #endregion

        #region Update Events
        private void hostName_TextChanged(object sender, EventArgs e)
        {
            configChanged = true;
            config.Host = hostName.Text;
        }

        private void usernameBox_TextChanged(object sender, EventArgs e)
        {
            configChanged = true;
            config.UserName = usernameBox.Text;
        }

        private void passwordBox_TextChanged(object sender, EventArgs e)
        {
            configChanged = true;
            config.Password = passwordBox.Text;
        }

        private void passwordBox2_TextChanged(object sender, EventArgs e)
        {
            configChanged = true;
        }

        private void rootFolderBox_TextChanged(object sender, EventArgs e)
        {
            configChanged = true;
            config.DefaultFolderName = rootFolderBox.Text;
        }

        private void localCacheBox_TextChanged(object sender, EventArgs e)
        {
            configChanged = true;
            config.CacheFile = localCacheBox.Text;
        }

        private void xmlFormat_CheckedChanged(object sender, EventArgs e)
        {
            configChanged = true;
            config.Format = xmlFormat.Checked ? CacheFormat.XML : CacheFormat.Binary;
        }

        private void binaryFormat_CheckedChanged(object sender, EventArgs e)
        {
            configChanged = true;
            config.Format = binaryFormat.Checked ? CacheFormat.Binary : CacheFormat.XML;
        }

        private void useSSL_CheckedChanged(object sender, EventArgs e)
        {
            configChanged = true;
            config.UseSSL = useSSL.Checked;
        }

        private void autoLogon_CheckedChanged(object sender, EventArgs e)
        {
            configChanged = true;
            config.AutoLogon = autoLogon.Checked;
        }

        private void debugMode_CheckedChanged(object sender, EventArgs e)
        {
            configChanged = true;
            config.DebugMode = debugMode.Checked;
        }

        private void autoLoad_CheckedChanged(object sender, EventArgs e)
        {
            configChanged = true;
            config.AutoGetMsgID = autoLoad.Checked;
        }

        private void autoSyncCacheBox_CheckedChanged(object sender, EventArgs e)
        {
            configChanged = true;
            config.AutoSyncCache = autoSyncCacheBox.Checked;
        }

        private void logFileBox_TextChanged(object sender, EventArgs e)
        {
            configChanged = true;
            config.LogFile = logFileBox.Text;
        }
        #endregion

        private void debugDetailBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            configChanged = true;
            switch (debugDetailBox.SelectedIndex)
            {
                case 1:
                    {
                        config.DebugDetail = IMAPConfig.DebugDetailLevel.All;
                        break;
                    }
                case 2:
                    {
                        config.DebugDetail = IMAPConfig.DebugDetailLevel.InterIMAPOnly;
                        break;
                    }
                case 3:
                    {
                        config.DebugDetail = IMAPConfig.DebugDetailLevel.All;
                        break;
                    }
                default:
                    {
                        config.DebugDetail = IMAPConfig.DebugDetailLevel.All;
                        break;
                    }
            }
        }

        

        

        

        
    }
}
