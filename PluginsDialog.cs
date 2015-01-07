using System;
using System.Reflection;
using System.Windows.Forms;

namespace CKAN
{
    public partial class PluginsDialog : Form
    {
        public PluginsDialog()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        private OpenFileDialog m_AddNewPluginDialog = new OpenFileDialog();

        private void PluginsDialog_Load(object sender, EventArgs e)
        {
            DeactivateButton.Enabled = false;
            ReloadPluginButton.Enabled = false;
            ActivatePluginButton.Enabled = false;
            UnloadPluginButton.Enabled = false;

            RefreshPlugins();

            m_AddNewPluginDialog.Filter = "CKAN Plugins (*.dll)|*.dll";
            m_AddNewPluginDialog.Multiselect = false;
        }

        private void RefreshPlugins()
        {
            var plugins = Main.Instance.m_PluginController.Plugins;

            ActivePluginsListBox.Items.Clear();
            DormantPluginsListBox.Items.Clear();

            foreach (var plugin in plugins)
            {
                if (plugin.IsActive)
                {
                    ActivePluginsListBox.Items.Add(plugin);
                }
                else
                {
                    DormantPluginsListBox.Items.Add(plugin);
                }
            }
        }

        private void ActivePluginsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool state = ActivePluginsListBox.SelectedItem != null;
            DeactivateButton.Enabled = state;
            ReloadPluginButton.Enabled = state;
        }

        private void DeactivateButton_Click(object sender, EventArgs e)
        {
            if (ActivePluginsListBox.SelectedItem == null)
            {
                return;
            }

            var plugin = (Plugin)ActivePluginsListBox.SelectedItem;
            plugin.Deactivate();
            RefreshPlugins();
        }

        private void ReloadPluginButton_Click(object sender, EventArgs e)
        {
            if (ActivePluginsListBox.SelectedItem == null)
            {
                return;
            }

            var plugin = (Plugin)ActivePluginsListBox.SelectedItem;
            plugin.Deactivate();
            plugin.Activate();
            RefreshPlugins();
        }

        private void DormantPluginsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool state = DormantPluginsListBox.SelectedItem != null;
            ActivatePluginButton.Enabled = state;
            UnloadPluginButton.Enabled = state;
        }

        private void ActivatePluginButton_Click(object sender, EventArgs e)
        {
            if (DormantPluginsListBox.SelectedItem == null)
            {
                return;
            }

            var plugin = (Plugin)DormantPluginsListBox.SelectedItem;
            plugin.Activate();
            RefreshPlugins();
        }

        private void UnloadPluginButton_Click(object sender, EventArgs e)
        {
            if (DormantPluginsListBox.SelectedItem == null)
            {
                return;
            }

            var plugin = (Plugin)DormantPluginsListBox.SelectedItem;
            Main.Instance.m_PluginController.UnloadPlugin(plugin);
        }

        private void AddNewPluginButton_Click(object sender, EventArgs e)
        {
            if (m_AddNewPluginDialog.ShowDialog() == DialogResult.OK)
            {
                var path = m_AddNewPluginDialog.FileName;
                Main.Instance.m_PluginController.AddNewAssemblyToPluginsPath(path);
                RefreshPlugins();
            }
        }

    }
}