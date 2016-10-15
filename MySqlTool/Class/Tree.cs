using System;
using System.Drawing;
using System.Windows.Forms;

namespace MySqlTool.Class
{
	public abstract class Tree
	{
		public static void Expand(TreeNode treeNode, int level)
		{
			treeNode.Expand();
			int num = 0;
			foreach (TreeNode treeNode2 in treeNode.Nodes)
			{
				if (++num > level)
				{
					break;
				}
				treeNode2.Expand();
			}
		}

		public static void Expand(TreeNode treeNode)
		{
			Tree.Expand(treeNode, 2);
		}

		public static void CopyTo(TreeView treeView0, TreeView treeView1)
		{
			foreach (TreeNode treeNode in treeView0.Nodes)
			{
				Tree.CopyTo(treeNode, treeView1);
			}
		}

		public static void CopyTo(TreeNode treeNode, TreeView treeView)
		{
			TreeNode treeNode2 = new TreeNode(treeNode.Text, treeNode.ImageIndex, treeNode.SelectedImageIndex);
			treeNode2.Tag = treeNode.Tag;
			treeNode2.ForeColor = treeNode.ForeColor;
			treeView.Nodes.Add(treeNode2);
			foreach (TreeNode treeNode3 in treeNode.Nodes)
			{
				Tree.CopyTo(treeNode3, treeNode2);
			}
		}

		public static void CopyTo(TreeNode treeNode0, TreeNode treeNode1)
		{
			TreeNode treeNode2 = new TreeNode(treeNode0.Text, treeNode0.ImageIndex, treeNode0.SelectedImageIndex);
			treeNode2.Tag = treeNode0.Tag;
			treeNode2.ForeColor = treeNode0.ForeColor;
			treeNode1.Nodes.Add(treeNode2);
			foreach (TreeNode treeNode3 in treeNode0.Nodes)
			{
				Tree.CopyTo(treeNode3, treeNode2);
			}
		}

		public static void SetBackColor(TreeView tree, TreeNode treeNode)
		{
			foreach (TreeNode treeNode2 in tree.Nodes)
			{
				treeNode2.BackColor = Color.White;
				treeNode2.ForeColor = Color.Black;
				Tree.SetBackColor(treeNode2);
			}
			treeNode.BackColor = Color.RoyalBlue;
			treeNode.ForeColor = Color.White;
		}

		private static void SetBackColor(TreeNode treeNode)
		{
			foreach (TreeNode treeNode2 in treeNode.Nodes)
			{
				treeNode2.BackColor = Color.White;
				treeNode2.ForeColor = Color.Black;
				Tree.SetBackColor(treeNode2);
			}
		}

		public static void SetCheck(TreeNode treeNode)
		{
			for (TreeNode parent = treeNode.Parent; parent != null; parent = parent.Parent)
			{
				bool flag = treeNode.Checked;
				if (!flag)
				{
					foreach (TreeNode treeNode2 in parent.Nodes)
					{
						if (treeNode2.Checked)
						{
							flag = true;
							break;
						}
					}
				}
				parent.Checked = flag;
			}
			Tree.SetSubCheck(treeNode);
		}

		private static void SetSubCheck(TreeNode treeNode)
		{
			foreach (TreeNode treeNode2 in treeNode.Nodes)
			{
				treeNode2.Checked = treeNode.Checked;
				Tree.SetSubCheck(treeNode2);
			}
		}
	}
}
