using NutDavClient.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Security.AccessControl;
using System.Text;
using System.Windows.Forms;

namespace NutDavClient
{
    public partial class MainFrm : Form
    {
        public MainFrm()
        {
            InitializeComponent();
        }

        private void PopulateTreeView()
        {
            ImageList imageList = new ImageList();
            imageList.Images.Add("ClosedFolder", (Image)(Resources.ResourceManager.GetObject("ClosedFolder")));
            imageList.Images.Add("OpenFolder", (Image)(Resources.ResourceManager.GetObject("OpenFolder")));

            tvFiles.ImageList = imageList;

            TreeNode rootNode;

            DirectoryInfo info = new DirectoryInfo(@"F:\Download");
            if (info.Exists)
            {
                rootNode = new TreeNode(info.Name);
                rootNode.Tag = info;
                GetDirectories(info.GetDirectories(), rootNode);
                tvFiles.Nodes.Add(rootNode);
            }
        }

        private void GetDirectories(DirectoryInfo[] subDirs, TreeNode nodeToAddTo)
        {
            TreeNode aNode;
            DirectoryInfo[] subSubDirs;
            foreach (DirectoryInfo subDir in subDirs)
            {
                if (subDir.Attributes.HasFlag(FileAttributes.Hidden) || subDir.GetAccessControl().AreAccessRulesProtected)
                {
                    continue;
                }

                aNode = new TreeNode(subDir.Name, 0, 0);
                aNode.Tag = subDir;
                aNode.ImageKey = "ClosedFolder";
                aNode.SelectedImageKey = "OpenFolder";
                subSubDirs = subDir.GetDirectories();
                if (subSubDirs.Length != 0)
                {
                    GetDirectories(subSubDirs, aNode);
                }

                nodeToAddTo.Nodes.Add(aNode);
            }
        }

        private void MainFrm_Load(object sender, EventArgs e)
        {
            PopulateTreeView();
        }
    }
}
