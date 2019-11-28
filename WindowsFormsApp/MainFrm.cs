using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp.Properties;
using WebDAVClient;
using System.Net;

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

        private string CalcTime(int sec)
        {
            if (sec < 60)
            {
                return sec + "sec";
            }
            else if (sec < (60 * 60))
            {
                int min = sec / 60;
                return min + "min" + (sec - min * 60) + "sec";
            }
            else
            {
                int hour = sec / (60 * 60);
                int min = (sec - (hour * 60 * 60)) / 60;
                return hour + "hour" + min + "min" + (sec - (hour * 60 * 60 + min * 60)) + "sec";
            }
        }

        #endregion

        private void MainFrm_Load(object sender, EventArgs e)
        {
            //Set ImageList

            ImageList imageList = new ImageList();
            imageList.Images.Add("ClosedFolder", (Image)(Resources.ResourceManager.GetObject("ClosedFolder")));
            imageList.Images.Add("OpenFolder", (Image)(Resources.ResourceManager.GetObject("OpenFolder")));
            imageList.Images.Add("file", (Image)(Resources.ResourceManager.GetObject("file")));

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
            SetData((DirectoryInfo)e.Node.Tag, (files) => files.OrderBy(x => x.Name));
        }

        private void SetData(DirectoryInfo directoryInfo, Func<FileInfo[], IEnumerable<FileInfo>> CustomRule)
        {
            FileInfo[] files = directoryInfo.GetFiles();
            IEnumerable<FileInfo> fileInfos = CustomRule.Invoke(files);

            lvFileExplorer.Items.Clear();

            lvFileExplorer.BeginUpdate();
            foreach (FileInfo file in fileInfos)
            {
                var item = new ListViewItem();
                item.ImageKey = "file";
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

        private async void btnSync_Click(object sender, EventArgs e)
        {
            try
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                //cts.CancelAfter(30 * 1000);

                DownloadHelper _HTTPHelper = new DownloadHelper(cts);
                _HTTPHelper.OnProgressHandler += HTTPHelper_OnProgressHandler;
                await _HTTPHelper.CrazyDownload(
                    "https://vscode.cdn.azure.cn/stable/f359dd69833dd8800b54d458f6d37ab7c78df520/VSCodeUserSetup-x64-1.40.2.exe"
                    , new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop))
                    , 3);

                pbFile.Value = 0;
                lblTip.Text = "Download successful";
                lblSpeed.Text = string.Empty;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    MessageBox.Show(ex.InnerException.Message);
                }

                MessageBox.Show(ex.Message);
            }
        }

        private void HTTPHelper_OnProgressHandler(string fileName, int costTime, double percentage, long speed)
        {
            pbFile.Invoke(new MethodInvoker(() => pbFile.Value = Convert.ToInt32(percentage * 100)));
            lblSpeed.Invoke(new MethodInvoker(() => lblSpeed.Text = CalcSize(speed) + "/s"));
            lblTip.Invoke(new MethodInvoker(() => lblTip.Text = fileName));
            lblTime.Invoke(new MethodInvoker(() => lblTime.Text = CalcTime(costTime)));
        }

        private void lvFileExplorer_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.Item.Checked && e.Item.Selected)
            {
                e.Item.Checked = true;
                e.Item.Selected = false;
            }
            else if (!e.Item.Checked)
            {
                e.Item.BackColor = Color.White;
            }
        }

        private void lvFileExplorer_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (!e.Item.Checked)
            {
                e.Item.BackColor = Color.White;
            }
            else
            {
                e.Item.BackColor = Color.FromArgb(50, 192, 168, 232);
            }
        }

        bool col0Status = false;
        bool col1Status = false;
        bool col2Status = false;
        bool col3Status = false;

        private void lvFileExplorer_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            //Order

            if (e.Column == 0)
            {
                if (col0Status == false)
                {
                    SetData((DirectoryInfo)tvFiles.SelectedNode.Tag, (files) => files.OrderBy(x => x.Name));
                }
                else
                {
                    SetData((DirectoryInfo)tvFiles.SelectedNode.Tag, (files) => files.OrderByDescending(x => x.Name));
                }
                col0Status = !col0Status;
            }
            else if (e.Column == 1)
            {
                if (col1Status == false)
                {
                    SetData((DirectoryInfo)tvFiles.SelectedNode.Tag,
                        (files) => files.OrderBy(x => x.Extension).ThenBy(x => x.Name));
                }
                else
                {
                    SetData((DirectoryInfo)tvFiles.SelectedNode.Tag,
                        (files) => files.OrderByDescending(x => x.Extension).ThenBy(x => x.Name));
                }
                col1Status = !col1Status;
            }
            else if (e.Column == 2)
            {
                if (col2Status == false)
                {
                    SetData((DirectoryInfo)tvFiles.SelectedNode.Tag,
                        (files) => files.OrderBy(x => x.LastWriteTime).ThenBy(x => x.Name));
                }
                else
                {
                    SetData((DirectoryInfo)tvFiles.SelectedNode.Tag,
                        (files) => files.OrderByDescending(x => x.LastWriteTime).ThenBy(x => x.Name));
                }
                col2Status = !col2Status;
            }
            else if (e.Column == 3)
            {
                if (col3Status == false)
                {
                    SetData((DirectoryInfo)tvFiles.SelectedNode.Tag,
                        (files) => files.OrderBy(x => x.Length).ThenBy(x => x.Name));
                }
                else
                {
                    SetData((DirectoryInfo)tvFiles.SelectedNode.Tag,
                        (files) => files.OrderByDescending(x => x.Length).ThenBy(x => x.Name));
                }
                col3Status = !col3Status;
            }
        }

        private async void btnTest_Click(object sender, EventArgs e)
        {
            // Basic authentication required

            // List items in the root folder
            var files = await c.List();
        }
    }
}
