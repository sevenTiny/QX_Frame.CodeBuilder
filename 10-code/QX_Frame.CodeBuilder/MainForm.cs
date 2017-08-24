﻿using CSharp_FlowchartToCode_DG.CodeCreate;
using QX_Frame.Helper_DG;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;


namespace CSharp_FlowchartToCode_DG
{
    public partial class MainForm : Form
    {
        #region 代码编辑全局变量
        public static Dictionary<string, dynamic> CreateInfoDic = new Dictionary<string, dynamic>();         //存储全部信息的List

        string DataBaseName = "DataBase1";                    //数据库名
        string TableName = "Table1";                          //表名
        List<string> FeildName = new List<string>();          //表字段名称
        List<string> FeildType = new List<string>();          //表字段类型
        List<string> FeildIsNullable = new List<string>();    //表字段可空
        List<string> FeildLength = new List<string>();        //表字段长度
        List<string> FeildDescription = new List<string>();   //表字段说明
        List<string> FeildIsPK = new List<string>();          //表字段是否主键
        List<string> FeildIsIdentity = new List<string>();    //表字段是否自增


        string CodeTxt = "";        //代码字符串，用于输出到文件
        string dir = IO_Helper_DG.DeskTopPath;                  //获取路径
        string filePath = @"qixiaoSrc\QixiaoConfig.ini";          //获取配置文件的路径
        DataTable DataBaseTable = default(DataTable); //数据库表数据DataTable

        #endregion

        public MainForm() => InitializeComponent();
        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                // set copyright
                label_Author.Text += Info.Author;
                label_Version.Text += Info.VersionNum;
                label_Description.Text += Info.Description;

                //set config
                textBox1.Text = File_Helper_DG.Ini_SelectStringValue(filePath, "config", "dataSource", ".");//data source
                comboBox1.Text = File_Helper_DG.Ini_SelectStringValue(filePath, "config", "loginType", "Integrated Security=True");// system or sa ?
                comboBox2.Text = File_Helper_DG.Ini_SelectStringValue(filePath, "config", "outputType");//output type

                //set code builder config
                // textBox3.Text = File_Helper_DG.Ini_SelectStringValue(filePath, "code", "usings").Replace('&','\n'); //using
                textBox2.Text = File_Helper_DG.Ini_SelectStringValue(filePath, "code", "namespace");//namespace
                textBox10.Text = File_Helper_DG.Ini_SelectStringValue(filePath, "code", "namespaceCommonPlus");//namespace
                textBox9.Text = File_Helper_DG.Ini_SelectStringValue(filePath, "code", "TableName");//table name
                textBox5.Text = File_Helper_DG.Ini_SelectStringValue(filePath, "code", "class");//class name 
                textBox7.Text = File_Helper_DG.Ini_SelectStringValue(filePath, "code", "ClassNamePlus");//ClassExtends
                textBox8.Text = File_Helper_DG.Ini_SelectStringValue(filePath, "code", "ClassExtends");//ClassExtends
                textBox6.Text = dir + "\\" + File_Helper_DG.Ini_SelectStringValue(filePath, "code", "filePathRelativeDeskTop") + "\\";//filePath
                textBox4.Text = File_Helper_DG.Ini_SelectStringValue(filePath, "code", "fileName");//fileName

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //write ini config record the opration history
        private void setInitConfigFile()
        {
            //set config
            File_Helper_DG.Ini_Update(filePath, "config", "dataSource", textBox1.Text);//data source
            File_Helper_DG.Ini_Update(filePath, "config", "loginType", comboBox1.Text);// system or sa ?
            File_Helper_DG.Ini_Update(filePath, "config", "outputType", comboBox2.Text);//output type

            //set code builder config
            //File_Helper_DG.Ini_Update(filePath, "code", "usings", textBox3.Text.Replace("\n","&")); //using
            File_Helper_DG.Ini_Update(filePath, "code", "namespace", textBox2.Text);//namespace
            File_Helper_DG.Ini_Update(filePath, "code", "namespaceCommonPlus", textBox10.Text);//namespace
            File_Helper_DG.Ini_Update(filePath, "code", "TableName", textBox9.Text);//table name
            File_Helper_DG.Ini_Update(filePath, "code", "class", textBox5.Text);//class name
            File_Helper_DG.Ini_Update(filePath, "code", "ClassNamePlus", textBox7.Text);// ClassExtends
            File_Helper_DG.Ini_Update(filePath, "code", "ClassExtends", textBox8.Text);// ClassExtends
            File_Helper_DG.Ini_Update(filePath, "code", "fileName", textBox4.Text);//fileName
        }

        #region 获取数据库结构的代码

        //获取数据库信息
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                string sql = "select name from sys.databases where database_id > 4";//查询sqlserver中的非系统库
                string ConnectionStr = $"Data Source={textBox1.Text.Trim()};Initial Catalog=master;{comboBox1.Text.Trim()};";
                DataSet ds = SqlHelper.ExecuteDataSet(ConnectionStr, sql);
                DataTable dataTable = ds.Tables[0];

                TreeNode grand = new TreeNode(textBox1.Text.Trim());//添加节点服务器地址
                grand.ImageIndex = 1;
                treeView1.Nodes[0].Nodes.Add(grand);

                foreach (DataRow row in dataTable.Rows)
                {
                    TreeNode root = new TreeNode(row["name"].ToString());//创建节点
                    root.Name = row["name"].ToString();
                    root.ImageIndex = 2;
                    grand.Nodes.Add(root);
                    TreeNode biao = new TreeNode("Tables");
                    biao.Name = "Tables";
                    biao.ImageIndex = 3;
                    root.Nodes.Add(biao);


                    //获取表名
                    //string sqltable = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
                    //DataSet ds2 = SqlHelper.ExecuteDataSet("Data Source=" + textBox1.Text.Trim() + ";Initial Catalog=" + row["name"] + ";Integrated Security=True", sqltable);
                    string sqlTable = $"use [{root.Name}] SELECT name FROM sysobjects WHERE xtype = 'U' AND OBJECTPROPERTY (id, 'IsMSShipped') = 0";
                    DataSet ds2 = SqlHelper.ExecuteDataSet(ConnectionStr, sqlTable);
                    DataTable dt2 = ds2.Tables[0];
                    List<string> tableNameList = new List<string>();
                    foreach (DataRow row2 in dt2.Rows)
                    {
                        tableNameList.Add(row2[0].ToString());
                    }
                    tableNameList.Sort();// sort the list
                    foreach (var item in tableNameList)
                    {
                        TreeNode biaovalue = new TreeNode(item);
                        biaovalue.Name = item;
                        biaovalue.ImageIndex = 4;
                        biao.Nodes.Add(biaovalue);
                    }
                }
                setInitConfigFile();//set init config file
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) => getTableInfo();

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e) => getTableInfo();

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e) => getTableInfo();

        //获取数据库表信息
        public void getTableInfo()
        {
            try
            {
                string database = this.treeView1.SelectedNode.Parent.Parent.Name;
                string table = this.treeView1.SelectedNode.Name;

                this.DataBaseName = database;//保存数据库名
                this.TableName = table;

                textBox5.Text = table;//将table的表名赋值给TableName变量，方便后续传值; Model
                textBox9.Text = table;//将table的表名赋值给TableName变量，方便后续传值; Model
                textBox4.Text = table + textBox7.Text.Trim() + ".cs";//fileName
                //string connStr = "Data Source=" + textBox1.Text.Trim() + ";Initial Catalog=" + database + ";Integrated Security=True";
                string ConnectionStr = $"Data Source={textBox1.Text.Trim()};Initial Catalog=master;{comboBox1.Text.Trim()};";
                string sql = $@"use [{DataBaseName}] select syscolumns.name as Field ,systypes.name as FieldType , syscolumns.length as Length,syscolumns.isnullable as Nullable, sys.extended_properties.value as Description  ,IsPK = Case  when exists ( select 1 from sysobjects  inner join sysindexes  on sysindexes.name = sysobjects.name  inner join sysindexkeys  on sysindexes.id = sysindexkeys.id  and  sysindexes.indid = sysindexkeys.indid  where xtype='PK'  and parent_obj = syscolumns.id and sysindexkeys.colid = syscolumns.colid ) then 1 else 0 end ,IsIdentity = Case syscolumns.status when 128 then 1 else 0 end  from syscolumns inner join systypes on(  syscolumns.xtype = systypes.xtype and systypes.name <>'_default_' and systypes.name<>'sysname'  ) left outer join sys.extended_properties on  ( sys.extended_properties.major_id=syscolumns.id and minor_id=syscolumns.colid  ) where syscolumns.id = (select id from sysobjects where name='" + table + @"') order by syscolumns.colid ";
                DataSet ds = SqlHelper.ExecuteDataSet(ConnectionStr, sql);

                DataTable dt = ds.Tables[0];
                this.DataBaseTable = dt;//将获取到的表信息保存到全局变量
                this.dataGridView1.DataSource = dt.DefaultView;
                //设置初始值为全选中
                for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                {
                    this.dataGridView1.Rows[i].Cells[0].Value = true;
                }
            }
            catch (Exception)
            {
                //throw;
                //MessageBox.Show("无法检索到字段！");
            }
        }
        #endregion

        #region 操作栏按钮点击事件 全选、清空、导出到Excel...

        //工具窗口按钮
        private void button1_Click(object sender, EventArgs e)=> new ToolsForm().Show();

        //全选按钮
        private void button4_Click(object sender, EventArgs e)
        {
            int count = Convert.ToInt32(this.dataGridView1.Rows.Count.ToString());
            for (int i = 0; i < count; i++)
            {
                //如果DataGridView是可编辑的，将数据提交，否则处于编辑状态的行无法取到 
                this.dataGridView1.EndEdit();
                DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)this.dataGridView1.Rows[i].Cells["Check"];
                checkCell.Value = true;
            }
        }

        //清空按钮
        private void button5_Click(object sender, EventArgs e)
        {
            int count = Convert.ToInt32(this.dataGridView1.Rows.Count.ToString());
            for (int i = 0; i < count; i++)
            {
                //如果DataGridView是可编辑的，将数据提交，否则处于编辑状态的行无法取到 
                this.dataGridView1.EndEdit();
                DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)this.dataGridView1.Rows[i].Cells["Check"];
                checkCell.Value = false;
            }
        }

        //Export To Excel
        private void button22_Click(object sender, EventArgs e)
        {
            string filePath = textBox6.Text.Trim();
            string fileComplexPath = $"{ filePath + DataBaseName}.xlsx";
            IO_Helper_DG.CreateDirectoryIfNotExist(filePath);
            new Thread(() =>
             {
                 Office_Helper_DG.DataTableToExcel(fileComplexPath, textBox9.Text.Trim(), this.DataBaseTable);
             }).Start();
            MessageBox.Show("OutPut->" + fileComplexPath);
        }
        //open code dir
        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                string dirPath = textBox6.Text;
                using (System.Diagnostics.Process.Start("Explorer.exe", dirPath)) { }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        #endregion

        #region page2

        //将文本框文件保存到桌面
        private void saveCodeToFile()
        {
            string dirPath = textBox6.Text;
            string fileComplexPath = dirPath + textBox4.Text;
            IO_Helper_DG.CreateDirectoryIfNotExist(dirPath);
            using (FileStream fs = new FileStream(fileComplexPath, FileMode.Create))
            {
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(CodeTxt);
                sw.Close();
            }
            //MessageBox.Show("OutPut->" + fileComplexPath);
        }

        private void button6_Click(object sender, EventArgs e) => saveCodeToFile();

        //返回代码生成设置页面 也就是首页
        private void button14_Click(object sender, EventArgs e) => this.tabControl1.SelectedTab = tabPage1;//转换到首页

        //全选的按钮 的事件
        private void button12_Click(object sender, EventArgs e) => richTextBox1.SelectAll();//全选

        //复制按钮的事件
        private void button13_Click(object sender, EventArgs e) => richTextBox1.Copy();//复制选择文本

        //left to right
        private void button19_Click(object sender, EventArgs e) => this.richTextBox1.RightToLeft = RightToLeft.No;

        //right to left
        private void button20_Click(object sender, EventArgs e) => this.richTextBox1.RightToLeft = RightToLeft.Yes;

        #endregion

        #region function button
        /// <summary>
        /// set button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button10_Click(object sender, EventArgs e) => ChangeTexBox4();
        private void textBox5_TextChanged(object sender, EventArgs e) => ChangeTexBox4();
        private void textBox7_TextChanged(object sender, EventArgs e) => ChangeTexBox4();
        private void ChangeTexBox4()
        {
            try
            {
                string[] createFileName = new string[2];
                if (textBox4.Text.Contains("."))
                {
                    createFileName = textBox4.Text.Trim().Split('.');
                }
                createFileName[0] = textBox5.Text.Trim() + textBox7.Text.Trim();
                textBox4.Text = $"{createFileName[0]}.{createFileName[1]}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        #endregion

        #region code builder settings
        //common set infoList
        private void SetInfoList()
        {
            //先将字段列表全部清空
            CreateInfoDic.Clear();

            FeildName.Clear();
            FeildType.Clear();
            FeildLength.Clear();
            FeildIsNullable.Clear();
            FeildDescription.Clear();
            FeildIsPK.Clear();
            FeildIsIdentity.Clear();

            int count = Convert.ToInt32(this.dataGridView1.Rows.Count.ToString());
            for (int i = 0; i < count; i++)
            {
                //如果DataGridView是可编辑的，将数据提交，否则处于编辑状态的行无法取到 
                this.dataGridView1.EndEdit();
                DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)this.dataGridView1.Rows[i].Cells["Check"];
                Boolean flag = Convert.ToBoolean(checkCell.Value);
                if (flag == true)     //查找被选择的数据行 
                {
                    //从 DATAGRIDVIEW 中获取数据项 
                    string FName = this.dataGridView1.Rows[i].Cells[1].Value.ToString().Trim();
                    FeildName.Add(FName);
                    string FType = this.dataGridView1.Rows[i].Cells[2].Value.ToString().Trim();
                    FeildType.Add(FType);
                    string FLength = this.dataGridView1.Rows[i].Cells[3].Value.ToString().Trim();
                    FeildLength.Add(FLength);
                    string FIsNullable = this.dataGridView1.Rows[i].Cells[4].Value.ToString().Trim();
                    FeildIsNullable.Add(FIsNullable);
                    string FMark = this.dataGridView1.Rows[i].Cells[5].Value.ToString().Trim();
                    FeildDescription.Add(FMark);
                    string FIsPK = this.dataGridView1.Rows[i].Cells[6].Value.ToString().Trim();
                    FeildIsPK.Add(FIsPK);
                    string FIsIdentity = this.dataGridView1.Rows[i].Cells[7].Value.ToString().Trim();
                    FeildIsIdentity.Add(FIsIdentity);
                }
            }
        }

        //builder common transmit settings
        private void InitCreateInfoDic()
        {
            SetInfoList();//设置信息

            CreateInfoDic.Add("Using", textBox3.Text.Trim());
            CreateInfoDic.Add("NameSpace", textBox2.Text.Trim());
            CreateInfoDic.Add("NameSpaceCommonPlus", textBox10.Text.Trim());
            CreateInfoDic.Add("DataBaseName", DataBaseName);
            CreateInfoDic.Add("TableName", textBox9.Text.Trim());
            CreateInfoDic.Add("Class", textBox5.Text.Trim());
            CreateInfoDic.Add("ClassNamePlus", textBox7.Text.Trim());
            CreateInfoDic.Add("ClassExtends", textBox8.Text.Trim());

            CreateInfoDic.Add("FeildName", FeildName);
            CreateInfoDic.Add("FeildType", FeildType);
            CreateInfoDic.Add("FeildLength", FeildLength);
            CreateInfoDic.Add("FeildIsNullable", FeildIsNullable);
            CreateInfoDic.Add("FeildDescription", FeildDescription);
            CreateInfoDic.Add("FeildIsPK", FeildIsPK);
            CreateInfoDic.Add("FeildIsIdentity", FeildIsIdentity);
        }

        //build bode common component Func
        private void CommonComponent(Func<string> method)
        {
            try
            {
                setInitConfigFile();//record the opration history
                InitCreateInfoDic();//init create info to dictionary

                richTextBox1.Text = null;

                CodeTxt = method();

                richTextBox1.Text = CodeTxt;    //获取代码
                if (comboBox2.Text.Equals("To File"))
                {
                    saveCodeToFile();//save code to file
                }
                else
                {
                    this.tabControl1.SelectedTab = tabPage2;//trasfer to code view
                }
                Timer1Start();//Start Timer1();
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.ToString());
            }
        }
        #endregion

        //Special Effects Timer
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (toolStripProgressBar1.Value < 100)
            {
                toolStripProgressBar1.Value += 10;
            }
            else
            {
                this.toolStripStatusLabel1.ForeColor = Color.Green;
                this.toolStripStatusLabel1.Text = "Generate Success!!!";
                this.timer1.Stop();
            }
        }
        //Init and Start timer
        private void Timer1Start()
        {
            if (timer1.Enabled)
            {
                timer1.Stop();
            }
            this.toolStripProgressBar1.Value = 0;
            this.toolStripStatusLabel1.ForeColor = Color.Red;
            this.toolStripStatusLabel1.Text = "Generate Watting...";
            this.timer1.Start();
        }

        #region CodeGenerate Opration
        //Entities
        private void button11_Click(object sender, EventArgs e)=>CommonComponent(() => CodeForEntity.CreateCode(CreateInfoDic));

        #endregion
        //Entities With Bantina
        private void button10_Click_1(object sender, EventArgs e)=> CommonComponent(() => CodeForEntityWithBantina.CreateCode(CreateInfoDic));
        //Entity-Instance
        private void button9_Click(object sender, EventArgs e)=> CommonComponent(() => CodeForInstance.CreateCode(CreateInfoDic));
        //Entity-Inst(FO)
        private void button3_Click(object sender, EventArgs e)=> CommonComponent(() => CodeForInstance.CreateCode_otherObject(CreateInfoDic));
        //SqlStatement
        private void button7_Click(object sender, EventArgs e) => CommonComponent(() => CodeForSqlStatement.CreateCode(CreateInfoDic));
        //QueryObject
        private void button18_Click(object sender, EventArgs e)=> CommonComponent(() => QX_FrameToQueryObject.CreateCode(CreateInfoDic));
        //Data.Contract
        private void button17_Click(object sender, EventArgs e)=> CommonComponent(() => QX_FrameToDataContract.CreateCode(CreateInfoDic));
        //Data.Service
        private void button16_Click(object sender, EventArgs e)=> CommonComponent(() => QX_FrameToDataService.CreateCode(CreateInfoDic));
        //Generate Three-Layout-Frame
        private void button15_Click(object sender, EventArgs e)
        {
            //Generate QX_Frame.Data.Entities
            textBox2.Text = "QX_Frame.Data.Entities";
            textBox7.Text = "";
            textBox8.Text = $"Entity<{DataBaseName}, {TableName}>";
            CommonComponent(() => CodeForEntityWithBantina.CreateCode(CreateInfoDic));
            //Generate QX_Frame.Data.QueryObject
            textBox3.Text = "using QX_Frame.App.Base;\nusing QX_Frame.Data.Entities;\nusing System;\nusing System.Linq.Expressions;";
            textBox2.Text = "QX_Frame.Data.QueryObject";
            textBox7.Text = "QueryObject";
            textBox8.Text = $"WcfQueryObject<{DataBaseName}, {TableName}>";
            CommonComponent(() => QX_FrameToQueryObject.CreateCode(CreateInfoDic));

            //Generate QX_Frame.Data.Service
            textBox3.Text = "using QX_Frame.App.Base;\nusing QX_Frame.Data.Contract;\nusing QX_Frame.Data.Entities;";
            textBox2.Text = "QX_Frame.Data.Service";
            textBox7.Text = "Service";
            string tableNameRelace = TableName.Replace("TB_", "").Replace("tb_", "").Replace("t_", "").Replace("T_", "");
            textBox5.Text = tableNameRelace;
            textBox8.Text = $"WcfService, I{tableNameRelace}Service";
            CommonComponent(() => QX_FrameToDataService.CreateCode(CreateInfoDic));

            string dirPath = textBox6.Text;
            string fileComplexPath = dirPath + "ClassRegister.txt";
            IO_Helper_DG.CreateDirectoryIfNotExist(dirPath);
            using (FileStream fs = new FileStream(fileComplexPath, FileMode.Create))
            {
                StreamWriter sw = new StreamWriter(fs);
                sw.Write($"AppBase.Register(c => new {tableNameRelace}Service());");
                sw.Close();
            }

            //Generate QX_Frame.Data.Contract
            textBox3.Text = "using QX_Frame.Data.Entities;";
            textBox2.Text = "QX_Frame.Data.Contract";
            textBox5.Text = $"I{textBox5.Text.Replace("TB_", "").Replace("tb_", "").Replace("t_", "").Replace("T_", "")}";
            textBox7.Text = "Service";
            textBox8.Text = "";
            CommonComponent(() => QX_FrameToDataContract.CreateCode(CreateInfoDic));
        }
        //REST WebApi Controller
        private void button21_Click(object sender, EventArgs e)=> CommonComponent(() => QX_FrameToRESTWebApiController.CreateCode(CreateInfoDic));
        //Jquery-Ajax
        private void button28_Click(object sender, EventArgs e)=> CommonComponent(() => CodeForJavascriptAjaxData.CreateCode(CreateInfoDic));
        //Jquery-Ajax-Data
        private void button27_Click(object sender, EventArgs e) => CommonComponent(() => CodeForJavascriptAjaxData.CreateCode(CreateInfoDic));
    }
}
