using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
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
            //Double Buffer          
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            InitializeComponent();
            PropertyInfo pi = lvFileExplorer.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(this.lvFileExplorer, true, null);
        }

        #region Funtion

        /// <summary>
        /// Get All Logical Drives
        /// </summary>
        private void PopulateTreeView()
        {
            foreach (var item in Environment.GetLogicalDrives())
            {
                TreeNode node = AddNode(item);
                if (node != null) tvFiles.Nodes.Add(node);
            }
        }

        /// <summary>
        /// Add nodes from path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get Directories and Add child node to parent node
        /// </summary>
        /// <param name="subDirs"></param>
        /// <param name="nodeToAddTo"></param>
        /// <param name="depth"></param>
        private void GetDirectories(DirectoryInfo[] subDirs, TreeNode nodeToAddTo, int depth)
        {
            if (depth > 1) return;

            TreeNode aNode;
            DirectoryInfo[] subSubDirs;
            foreach (DirectoryInfo subDir in subDirs)
            {
                if (subDir.Attributes.HasFlag(FileAttributes.Hidden))
                //|| subDir.GetAccessControl().AreAccessRulesProtected)
                {
                    continue;
                }

                try
                {
                    aNode = new TreeNode(subDir.Name, 0, 0);
                    aNode.Name = subDir.Name;
                    aNode.Tag = subDir;
                    aNode.ImageKey = "ClosedFolder";
                    aNode.SelectedImageKey = "OpenFolder";
                    subSubDirs = subDir.GetDirectories();
                    if (subSubDirs.Length != 0)
                    {
                        GetDirectories(subSubDirs, aNode, depth + 1);
                    }

                    //if (!(nodeToAddTo.Nodes.Find(aNode.Name, false).Count() > 0))
                    //{
                    nodeToAddTo.Nodes.Add(aNode);
                    //}
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GetDirectories {subDir.Name} {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Auto Resize Columns 
        /// </summary>
        private void CalcListViewWidth()
        {
            //Auto Resize Columns 
            lvFileExplorer.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

            //Get cols' sum
            int with = 0;
            foreach (ColumnHeader item in lvFileExplorer.Columns)
            {
                with += item.Width;
            }

            if (lvFileExplorer.Width < with)
            {
                lvFileExplorer.Columns[0].Width =
                    lvFileExplorer.Width
                    - lvFileExplorer.Columns[1].Width
                    - lvFileExplorer.Columns[2].Width
                    - lvFileExplorer.Columns[3].Width - 30;
            }
        }

        /// <summary>
        /// Get pleasant unit
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        private string CalcSize(long length)
        {
            int i = 1;
            double res = double.NaN;
            for (; i < 5; i++)
            {
                if (length < (1L << (i * 10)))
                {
                    res = length * 1.0 / (1L << ((i - 1) * 10));
                    break;
                }
            }

            res = Math.Round(res, 2);

            switch (i)
            {
                case 1:
                    return res + "Byte";
                case 2:
                    return res + "KB";
                case 3:
                    return res + "MB";
                case 4:
                    return res + "GB";
                case 5:
                    return res + "TB";
                default:
                    return length + "Byte";
            }
        }

        #endregion


        private void MainFrm_Load(object sender, EventArgs e)
        {
            //Set ImageList

            ImageList imageList = new ImageList();
            imageList.Images.Add("ClosedFolder", (Image)(Resources.ResourceManager.GetObject("ClosedFolder")));
            imageList.Images.Add("OpenFolder", (Image)(Resources.ResourceManager.GetObject("OpenFolder")));

            tvFiles.ImageList = imageList;
            lvFileExplorer.LargeImageList = imageList;
            lvFileExplorer.SmallImageList = imageList;

            //Set Listview Column Header
            this.lvFileExplorer.Columns.AddRange(new List<ColumnHeader>()
            {
                new ColumnHeader ()
                {
                    Text = "Name",
                    TextAlign = HorizontalAlignment.Left,
                    Width = 80
                },
                new ColumnHeader ()
                {
                    Text = "Ext",
                    TextAlign = HorizontalAlignment.Left,
                    Width = 80
                },
                new ColumnHeader ()
                {
                    Text = "LastWriteTime",
                    TextAlign = HorizontalAlignment.Left,
                    Width = 80
                },
                new ColumnHeader ()
                {
                    Text = "Length",
                    TextAlign = HorizontalAlignment.Left,
                    Width = 80
                }
            }.ToArray());

            PopulateTreeView();
        }

        private void tvFiles_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            //Reset selected Nodes childNodes
            List<TreeNode> treeNodeCollection = new List<TreeNode>();
            for (int i = 0; i < e.Node.Nodes.Count; i++)
            {
                treeNodeCollection.Add(AddNode(((DirectoryInfo)e.Node.Nodes[i].Tag).FullName));
            }
            e.Node.Nodes.Clear();
            e.Node.Nodes.AddRange(treeNodeCollection.ToArray());

            //Set data to listview
            DirectoryInfo directoryInfo = (DirectoryInfo)e.Node.Tag;
            FileInfo[] files = directoryInfo.GetFiles();

            lvFileExplorer.Items.Clear();

            lvFileExplorer.BeginUpdate();
            foreach (FileInfo file in files)
            {
                var item = new ListViewItem();
                item.ImageIndex = 1;
                item.Text = file.Name;
                item.Name = file.Name;

                item.SubItems.Add(file.Extension);
                item.SubItems.Add(file.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"));
                item.SubItems.Add(CalcSize(file.Length));

                lvFileExplorer.Items.Add(item);
            }
            lvFileExplorer.EndUpdate();

            //Auto cols with
            CalcListViewWidth();
        }

        private void MainFrm_SizeChanged(object sender, EventArgs e)
        {
            CalcListViewWidth();
        }

        private void btnSync_Click(object sender, EventArgs e)
        {
            MessageBox.Show(lvFileExplorer.CheckedItems.Count.ToString());
        }

        private void lvFileExplorer_MouseClick(object sender, MouseEventArgs e)
        {
            ListViewItem lv = lvFileExplorer.GetItemAt(e.X, e.Y);
            lv.Checked = !lv.Checked;
            if (lv.Checked)
            {
                lv.BackColor = Color.FromArgb(50,192, 168, 232);
                lv.Selected = false;
            }
            else
            {
                lv.BackColor = Color.White;
            }
        }
    }
}
