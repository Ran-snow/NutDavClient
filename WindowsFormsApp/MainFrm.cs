using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp.Properties;

namespace WindowsFormsApp
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
            lvFileExplorer.LargeImageList = imageList;

            foreach (var item in Environment.GetLogicalDrives())
            {
                TreeNode node = AddNode(item);
                if (node != null) tvFiles.Nodes.Add(node);
            }
        }

        private TreeNode AddNode(string path)
        {
            DirectoryInfo info = new DirectoryInfo(path);
            if (!info.Exists)
            {
                return null;
            }

            TreeNode node = new TreeNode(info.Name);
            node.Tag = info;
            GetDirectories(info.GetDirectories(), node, 0);

            return node;
        }

        private void GetDirectories(DirectoryInfo[] subDirs, TreeNode nodeToAddTo, int depth)
        {
            if (depth > 1) return;

            TreeNode aNode;
            DirectoryInfo[] subSubDirs;
            foreach (DirectoryInfo subDir in subDirs)
            {
                if (subDir.Attributes.HasFlag(FileAttributes.Hidden) || subDir.GetAccessControl().AreAccessRulesProtected)
                {
                    continue;
                }

                aNode = new TreeNode(subDir.Name, 0, 0);
                aNode.Name = subDir.Name;
                aNode.Tag = subDir;
                aNode.ImageKey = "ClosedFolder";
                aNode.SelectedImageKey = "OpenFolder";
                subSubDirs = subDir.GetDirectories();
                if (subSubDirs.Length != 0)
                {
                    GetDirectories(subSubDirs, aNode, ++depth);
                }

                if (!(nodeToAddTo.Nodes.Find(aNode.Name, false).Count() > 0))
                {
                    nodeToAddTo.Nodes.Add(aNode);
                }
            }
        }

        private void MainFrm_Load(object sender, EventArgs e)
        {
            PopulateTreeView();
        }

        private void tvFiles_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            e.Node.Nodes.Add(AddNode(((DirectoryInfo)e.Node.Tag).FullName));

            Console.WriteLine(e.Node.Text);
        }
    }
}
