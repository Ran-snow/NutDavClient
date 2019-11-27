using DAL;
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

        private void TvFiles_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void PopulateTreeView()
        {
            ImageList imageList = new ImageList();
            imageList.Images.Add("ClosedFolder", (Image)(Resources.ResourceManager.GetObject("ClosedFolder")));
            imageList.Images.Add("OpenFolder", (Image)(Resources.ResourceManager.GetObject("OpenFolder")));

            tvFiles.ImageList = imageList;
            lvFileExplorer.LargeImageList = imageList;

            foreach (var item in Environment.GetLogicalDrives())
            {
                DirectoryInfo info = new DirectoryInfo(item);
                if (info.Exists)
                {
                    TreeNode rootNode = new TreeNode(info.Name);
                    rootNode.Tag = info;
                    GetDirectories(info.GetDirectories(), rootNode, 0);
                    tvFiles.Nodes.Add(rootNode);
                }
            }
        }

        private void GetDirectories(DirectoryInfo[] subDirs, TreeNode nodeToAddTo, int depth)
        {
            if (depth > 2) return;

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
                    GetDirectories(subSubDirs, aNode, ++depth);
                }

                nodeToAddTo.Nodes.Add(aNode);
            }
        }

        private void MainFrm_Load(object sender, EventArgs e)
        {
            PopulateTreeView();
        }

        private void button1_Click(object sender, EventArgs e)
        {
           
        }
    }
}
