using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace MyFormRecg
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            
        }

        public ArrayList MyStrokeArrayWrite = new ArrayList();//该链表用于记录笔画类型，和笔画共存亡
        public int FormFlag = 1;//表格笔画标识
        public int StrokeDivFlag = 0;//笔画是否需要分割标识
        //public ArrayList MyVectorMulAdd = new ArrayList();//用于表示矢量积
        public ArrayList StrokeBoxHypLen = new ArrayList();//用于表示笔画外接矩形的长度
        //下面几个变量是为表格分析做准备的
        public ArrayList ArrayNewVectK = new ArrayList();//笔画分割后，新的表格笔画的斜率
        public ArrayList ArrayHandV = new ArrayList();//笔画分割后，表格笔画的方向，横或者竖
        public ArrayList ArrayP1Position = new ArrayList();//每条表格线段起点的坐标值
        public ArrayList ArrayP2Position = new ArrayList();//每条表格线段终点所谓坐标值
        public ArrayList StrokeDirectMax = new ArrayList();//记录最长的N个笔画的的方向链表
        public ArrayList StrokeBoundMax = new ArrayList();//链表用来记录最长的几笔笔画的外接矩形信息
        public ArrayList StrokeBoundX = new ArrayList();
        public ArrayList StrokeBoundY = new ArrayList();
        public ArrayList StrokeLengthMax = new ArrayList();//链表用来记录最长的几笔笔画的长度信息
        public double maxHorizonLen, minHorizonLen, maxVerticalLen, minVerticalLen;
        public int GVcnt =0;//变量定义全局笔画数
        public ArrayList StrokeLineH = new ArrayList();
        public ArrayList StrokeLineV = new ArrayList();
        public ArrayList CenterPoint = new ArrayList();
        public ArrayList StrokeLinePt = new ArrayList();
        public Collection<Collection<Int32>> AreaResult = new Collection<Collection<int>>();
        public int PureWord = new int();


        private void buttoncnt_Click(object sender, RoutedEventArgs e)//记录笔画数
        {
            int cnt = MyIP.Strokes.Count;
            if (cnt == 0) return;
            textBoxstrokescnt.Text = Convert.ToString(cnt);

        }

        private void buttonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)//清除功能
        {
            MyIP.Strokes.Clear();
            MyStrokeArrayWrite.Clear();
            MyIP.Children.Clear();
            if(radioButtonForm.IsChecked!=true)
            {
                radioButtonForm.IsChecked = true;
            }
        }

        private void button_IsForm(object sender, MouseButtonEventArgs e)
        {
            //textBox_IsForm.Text = Convert.ToString(MyIsForm());
              int typeflag = 0;//标记是表格还是文本
              int cnt = MyIP.Strokes.Count;
              Stroke st = MyIP.Strokes[cnt - 1];
              if (cnt == 0)
              {
                  MyStrokeArrayWrite.Clear();
                  return;
              }
          
              else
              { }
           if (radioButtonForm.IsChecked == true)
            {

                st.DrawingAttributes.Color = Colors.Yellow;
                typeflag = 1;
                MyStrokeArrayWrite.Add(typeflag);
                
            }
            else// if (radioButtonText.IsChecked == true)
            {
                st.DrawingAttributes.Color = Colors.White;
                typeflag = 0;
                MyStrokeArrayWrite.Add(typeflag);
                
            }
        }

        public int MyIsForm()//判断是否为表格笔画
        {
            int cnt = MyIP.Strokes.Count;
            if (cnt == 0) return 0;
            Stroke st = MyIP.Strokes[cnt - 1];
            int ptcnt = st.StylusPoints.Count;
            StylusPoint sp = st.StylusPoints[0];
            int flag = 0;
            Rect Stroke_Bound = st.GetBounds();
            double Box_Hypotenuse;
            Box_Hypotenuse = Math.Sqrt(Stroke_Bound.Height * Stroke_Bound.Height + Stroke_Bound.Width * Stroke_Bound.Width);

            if (Box_Hypotenuse > 100)
            {
                flag = 1;
            }
            else
            {
                flag = 0;
            }
            return flag;
        }

        private void buttonOpen_Click(object sender, RoutedEventArgs e)//打开文件
        {
            if (MyIP.EditingMode != InkCanvasEditingMode.Ink)
            {
                MyIP.EditingMode = InkCanvasEditingMode.Ink;
            }


            int cnt_old = MyIP.Strokes.Count;
            if (cnt_old != 0)
           {
                MyIP.Strokes.Clear();

            }
            MyIP.Children.Clear();
            MyIP.Strokes.Clear();
            MyStrokeArrayWrite.Clear();

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = "FormDatas"; // Default file name
            dlg.DefaultExt = ".imm"; // Default file extension
            dlg.Filter = "Form Datas(.imm)|*.imm|All File|*.*"; // Filter files by extension 

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Open document 
                string mypath = dlg.FileName;
                FileStream myfs = File.OpenRead(mypath);
                BinaryReader sb = new BinaryReader(myfs, Encoding.Default);
                myfs.Seek(0, SeekOrigin.Begin);

                int InfoHeader = sb.ReadInt32();
                int cnt = sb.ReadInt32();
                int ptcnt = 0;
               // MyIP.Strokes.Clear();
                MyStrokeArrayWrite.Clear();
                for (int i = 0; i < cnt; i++)
                {
                    ptcnt = sb.ReadInt32();
                    MyStrokeArrayWrite.Add(sb.ReadInt32());
                    StylusPoint[] MySP = new StylusPoint[ptcnt];

                    for (int j = 0; j < ptcnt; j++)
                    {
                        MySP[j] = new StylusPoint(sb.ReadDouble(), sb.ReadDouble());

                    }
                    StylusPointCollection MySPC = new StylusPointCollection(MySP);
                    Stroke newStroke = new Stroke(MySPC);
                   /* if (Convert.ToInt32(MyStrokeArrayWrite[i]) == 1)
                    {
                        newStroke.DrawingAttributes.Color = Colors.Yellow;
                    }
                    else
                    {
                        newStroke.DrawingAttributes.Color = Colors.White;
                    }*/
                    newStroke.DrawingAttributes.Color = Colors.Yellow;

                    MyIP.Strokes.Add(newStroke);
                   // MyIP.Strokes[i].DrawingAttributes.Color = Colors.Yellow;
                  //  if (i == cnt -1)
                  //  {
                 //       System.Threading.Thread.Sleep(500); 
 
                 //   }
                }

                //System.Threading.Thread.Sleep(500);
                //sb.ReadInt32();
                sb.Close();
                myfs.Close();

            }
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)//保存文件
        {
            if (radioButtonForm.IsChecked != true)
            {
                radioButtonForm.IsChecked = true;
            }
            
            SaveFileDialog mysfd = new SaveFileDialog();
            mysfd.CreatePrompt = true;
            mysfd.OverwritePrompt = true;
            mysfd.FileName = "FormDatas"; // Default file name
            mysfd.DefaultExt = ".imm"; // Default file extension
            mysfd.Filter = "Form Datas(.imm)|*.imm"; // Filter files by extension 
            // Show save file dialog box
            Nullable<bool> result = mysfd.ShowDialog();

            // Process save file dialog box results 
            if (result == true)
            {
                // Save document

                string filename = mysfd.FileName;


                FileStream myfs = File.Open(filename, FileMode.Create, FileAccess.Write);

                BinaryWriter sb = new BinaryWriter(myfs, Encoding.Default);



                //信息头，值分为0、1、2、3依次表示全文笔画、表格笔画、文本笔画、空余不用
                //这部分功能尚未开发
                int InfoHeader;
                InfoHeader = 0;
                sb.Write(InfoHeader);

                //笔画数
                int cnt = MyIP.Strokes.Count;
                sb.Write(cnt);

                //
                if (cnt == 0)
                {
                    sb.Close();
                    myfs.Close();
                    return;
                }

                //往文件中写数据
                Stroke st;
                int ptcnt = 0;
                //string mystr = "";
                for (int i = 0; i < cnt; i++)
                {
                    st = MyIP.Strokes[i];
                    ptcnt = st.StylusPoints.Count;
                    sb.Write(ptcnt);
                    sb.Write(Convert.ToInt32(MyStrokeArrayWrite[i]));

                    for (int j = 0; j < ptcnt; j++)
                    {
                        sb.Write(st.StylusPoints[j].X);
                        //mystr = mystr + " "+Convert.ToString(st.StylusPoints[j].Y)+"\n";
                        sb.Write(st.StylusPoints[j].Y);
                    }



                }
                //sb.Write(0);


                sb.Close();//Close;
                myfs.Close();
                
                
               
            }
        }

        private void buttonStandDiv_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < MyIP.Strokes.Count; i++)
            {
                Stroke st = MyIP.Strokes[i];
                if (Convert.ToString(MyStrokeArrayWrite[i] )!= "")
                {
                    if (Convert.ToInt32(MyStrokeArrayWrite[i]) == 1)
                    {
                        st.DrawingAttributes.Color = Colors.Yellow;
                    }
                    else
                    {
                        st.DrawingAttributes.Color = Colors.White;
                    }
 
                }
            }

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < MyIP.Strokes.Count; i++)
            {
                Stroke st = MyIP.Strokes[i];
                st.DrawingAttributes.Color = Colors.Yellow;
                
            }

        }

       /* private void buttonTestInput_Click(object sender, RoutedEventArgs e)//测试输入
        {
            string str;
            str = "";
            int readcnt = MyStrokeArrayWrite.Count;
            for (int i = 0; i < readcnt; i++)
            {
                str = str + MyStrokeArrayWrite[i];//标记的输入

            }
            textBoxTestInput.Text = str;
        }*/

       /* private void buttonFormPickup_Click(object sender, RoutedEventArgs e)//图文分割
        {
            for (int k = 0; k < MyIP.Strokes.Count; k++)
            {
                Stroke st = MyIP.Strokes[k];
                Rect Stroke_Bound = st.GetBounds();
                double Box_Hypotenuse;
                Box_Hypotenuse = Math.Sqrt(Stroke_Bound.Height * Stroke_Bound.Height + Stroke_Bound.Width * Stroke_Bound.Width);

                if (Box_Hypotenuse < 100)//不是表格笔画
                {
                    //MyIP.Strokes.Remove(st);
                    //MyStrokeArrayWrite.RemoveAt(k);
                    //k = k - 1;
                    st.DrawingAttributes.Color = Colors.Brown;
                    FormFlag = 0;
                }
                else
                {
                    FormFlag = 1;
                }

                if (((Stroke_Bound.Height) / Box_Hypotenuse > 0.2) && ((Stroke_Bound.Width) / Box_Hypotenuse > 0.2))
                {
                    StrokeDivFlag = 1;
                }
                else {
                    StrokeDivFlag = 0;
 
                }
            }
        }*/

        private void buttonSmooth_Click(object sender, RoutedEventArgs e)//笔画平滑，类似中值滤波形式
        {
            int cnt = MyIP.Strokes.Count;
            int ptcnt = 0;
            ArrayList dTmpDataX = new ArrayList();
            ArrayList dTmpDataY = new ArrayList();
            for (int i = 0; i < cnt;i++ )
            {
                Stroke st = MyIP.Strokes[i];
                ptcnt = st.StylusPoints.Count;
                if (ptcnt < 3)
                {
                    return;
                }
                dTmpDataX.Add(st.StylusPoints[0].X);
                dTmpDataY.Add(st.StylusPoints[0].Y);
                dTmpDataX.Add(st.StylusPoints[1].X);
                dTmpDataY.Add(st.StylusPoints[1].Y);
                for (int j = 2; j < ptcnt - 2;j++ )
                {
                    dTmpDataX.Add (0.2 * ((st.StylusPoints[j - 2].X) + (st.StylusPoints[j - 1].X) + (st.StylusPoints[j].X) + (st.StylusPoints[j + 1].X) + (st.StylusPoints[j + 2].X)));
                    dTmpDataY.Add (0.2 * ((st.StylusPoints[j - 2].Y) + (st.StylusPoints[j - 1].Y) + (st.StylusPoints[j].Y) + (st.StylusPoints[j + 1].Y) + (st.StylusPoints[j + 2].Y)));
                    
                }
                dTmpDataX.Add(st.StylusPoints[ptcnt-2].X);
                dTmpDataX.Add(st.StylusPoints[ptcnt-1].X);
                dTmpDataY.Add(st.StylusPoints[ptcnt - 2].Y);
                dTmpDataY.Add(st.StylusPoints[ptcnt - 1].Y);
                for (int k = 0; k < ptcnt ;k++ )
                {
                    //下面这种写法通不过编译
                    //st.StylusPoints[k].X = Convert.ToDouble(dTmpDataX[k]);
                    //st.StylusPoints[k].Y = Convert.ToDouble(dTmpDataY[k]);
                    StylusPoint sp = st.StylusPoints[k];
                    sp.X = Convert.ToDouble(dTmpDataX[k]);
                    sp.Y = Convert.ToDouble(dTmpDataY[k]);
                    st.StylusPoints[k] = sp;

                }
                dTmpDataX.Clear();//由于是在循环外设置的链表
                dTmpDataY.Clear();//每次循环之后需要对链表进行清空操作
            }
        }

        public void IsFormFlag(int i,ref int FormFlag,ref int StrokeFlag)
        {
            FormFlag = 0;
            StrokeFlag = 0;
            Stroke st = MyIP.Strokes[i];
            Rect Stroke_Bound = st.GetBounds();
            double Box_Hypotenuse;
            Box_Hypotenuse = Math.Sqrt(Stroke_Bound.Height * Stroke_Bound.Height + Stroke_Bound.Width * Stroke_Bound.Width);

            if (Box_Hypotenuse > 100)//是表格笔画
            {

                FormFlag = 1;//是否是表格笔画的标识（最简单的长度判别方法）
            }
            if (((Stroke_Bound.Height) / Box_Hypotenuse >= 0.2) && ((Stroke_Bound.Width) / Box_Hypotenuse >= 0.2))
            {
                StrokeFlag = 1;//是否需要对笔画分割的标识
            }
            return;
            
            //return MyFormStrokeFlag;
        }

        //public int IsStrokeDivFlag(int i)
        //{

        //}

        private void buttonStrokeDiv_Click(object sender, RoutedEventArgs e)//笔画分割
        {
            string strx, stry, strmuladd, strveckdiff, strdivloc;
            strx = "";
            stry = "";
            strmuladd = "";
            strveckdiff = "";
            strdivloc = "";
            int cnt = MyIP.Strokes.Count;
            int ptcnt = 0;
            double m, s, vectK;
            double MidVal = 0;
            int MaxLocation;
            //MyVectorMulAdd.Clear();
            int FormFlag, StrokeFlag;
            FormFlag = 0;
            StrokeFlag = 0;

            for (int i = 0; i < cnt; i++)
            {
                IsFormFlag(i, ref FormFlag, ref StrokeFlag);
                if (FormFlag == 1)
                {
                    if (StrokeFlag == 1)
                    {
                        Stroke st = MyIP.Strokes[i];
                        ptcnt = st.StylusPoints.Count;
                        ArrayList ArrayVectK = new ArrayList();
                        ArrayList VectKDiff = new ArrayList();
                        for (int k = 0; k < ptcnt - 10; k++)
                        {
                            strx = strx + " " + st.StylusPoints[k].X;
                            stry = stry + " " + st.StylusPoints[k].Y;
                            //MyVectorMulAdd.Add((st.StylusPoints[k + 1].X - st.StylusPoints[k].X) * (st.StylusPoints[k + 2].X - st.StylusPoints[k + 1].X) + (st.StylusPoints[k + 1].Y - st.StylusPoints[k].Y) * (st.StylusPoints[k +2].Y - st.StylusPoints[k+1].Y));
                            //strmuladd = strmuladd + " " + Convert.ToDouble(MyVectorMulAdd[k]);
                            //strmuladd = strmuladd + " " + (st.StylusPoints[k+1].X - st.StylusPoints[k].X);
                            m = st.StylusPoints[k + 10].X - st.StylusPoints[k].X;
                            s = st.StylusPoints[k + 10].Y - st.StylusPoints[k].Y;
                            //strmuladd = strmuladd + " " + n/(n+t);
                            vectK = (Math.Atan(s / m)) * 180 / (Math.PI);//斜率值
                            if (m < 0)
                            {
                                vectK += 180;
                            }
                            strmuladd = strmuladd + " " + Convert.ToInt32(vectK);
                            ArrayVectK.Add(vectK);
                        }
                        for (int t = 0; t < ptcnt - 11; t++)
                        {
                            VectKDiff.Add(Convert.ToDouble(ArrayVectK[t + 1]) - Convert.ToDouble(ArrayVectK[t]));//斜率向量的一阶导数
                            strveckdiff = strveckdiff + " " + Convert.ToInt32(VectKDiff[t]);
                        }

                        MidVal = Convert.ToDouble(VectKDiff[0]);
                        MaxLocation = 0;
                        for (int n = 0; n < ptcnt - 11; n++)//
                        {
                            if (Math.Abs(Convert.ToDouble(VectKDiff[n])) > MidVal)
                            {
                                MidVal = Math.Abs(Convert.ToDouble(VectKDiff[n]));
                                MaxLocation = n;
                            }


                        }

                        //笔画转角的平滑程度,1表示比较尖锐，-1表示比较平滑
                        if ((MidVal > 8) && (MaxLocation > 2))
                        {
                            if ((Convert.ToDouble(VectKDiff[MaxLocation - 1]) > 6) && (Convert.ToDouble(VectKDiff[MaxLocation + 1]) > 6))
                            {
                                strdivloc = MaxLocation + " " + MidVal + " " + st.StylusPoints[MaxLocation + 4].X + " " + st.StylusPoints[MaxLocation + 4].Y + " " + 1;

                            }
                            else
                            {
                                strdivloc = MaxLocation + " " + MidVal + " " + st.StylusPoints[MaxLocation + 4].X + " " + st.StylusPoints[MaxLocation + 4].Y + " " + 0;
                            }
                        }
                        else
                        {
                            strdivloc = MaxLocation + " " + MidVal + " " + st.StylusPoints[MaxLocation + 4].X + " " + st.StylusPoints[MaxLocation + 4].Y + " " + -1;
                        }

                        //创建新笔画，删除旧笔画，需要注意标志的重写
                        MaxLocation = MaxLocation + 5;

                        //第一笔
                        StylusPoint[] MySp = new StylusPoint[MaxLocation];
                        for (int fststrk = 0; fststrk < MaxLocation; fststrk++)
                        {
                            MySp[fststrk] = new StylusPoint(st.StylusPoints[fststrk].X, st.StylusPoints[fststrk].Y);
                        }
                        StylusPointCollection MySpc = new StylusPointCollection(MySp);

                        Stroke newStroke = new Stroke(MySpc);
                        newStroke.DrawingAttributes.Color = Colors.Yellow;

                        //第二笔
                        StylusPoint[] MySp2 = new StylusPoint[ptcnt - MaxLocation];
                        for (int secstrk = 0; secstrk < ptcnt - MaxLocation; secstrk++)
                        {
                            MySp2[secstrk] = new StylusPoint(st.StylusPoints[MaxLocation + secstrk].X, st.StylusPoints[MaxLocation + secstrk].Y);
                        }
                        StylusPointCollection MySpc2 = new StylusPointCollection(MySp2);
                        Stroke newStroke2 = new Stroke(MySpc2);
                        newStroke2.DrawingAttributes.Color = Colors.White;


                        MyIP.Strokes.RemoveAt(i);
                        MyStrokeArrayWrite.RemoveAt(i);
                        MyIP.Strokes.Add(newStroke);
                        MyStrokeArrayWrite.Add(1);
                        MyIP.Strokes.Add(newStroke2);
                        MyStrokeArrayWrite.Add(1);

                        ArrayVectK.Clear();
                        VectKDiff.Clear();
                        if (cnt <= 1)
                        {
                            return;
                        }
                        cnt = cnt - 1;//未来的某一天我会看到，调出这个-1的bug花了两个钟头
                        i = i - 1;
                    }
                }

            }

            //创立新笔画的一阶导数也就是方向的值



          //  textBox1.Text = strx;
          //  textBox2.Text = stry;
          //  textBox3.Text = strmuladd;
          //  textBoxVecKDiff.Text = strveckdiff;
          //  textBoxDivLoc.Text = strdivloc;


        }

       /* private void buttonDivStren_Click(object sender, RoutedEventArgs e)//改进的图文分割
        {
            string strz;
            strz = "";
            for (int k = 0; k < MyIP.Strokes.Count; k++)
            {
                Stroke st = MyIP.Strokes[k];
                Rect Stroke_Bound = st.GetBounds();
                double Box_Hypotenuse;
                Box_Hypotenuse = Math.Sqrt(Stroke_Bound.Height * Stroke_Bound.Height + Stroke_Bound.Width * Stroke_Bound.Width);
                strz = strz + " " + (int)Box_Hypotenuse;
                StrokeBoxHypLen.Add(Box_Hypotenuse);
            }
            
            //StrokeBoxHypLen.Sort();//其实排序要是能还原就好啦。。。
            int strcnt = StrokeBoxHypLen.Count;
            int i,j,m, max,mid,min;
            max = 0;
            mid =0;
            min =0;
            double  maxValue;
            double  maxValueBackUp1,maxValueBackUp2;
            int maxLoc;
            maxValue =(double)StrokeBoxHypLen[0];//长的笔画
            maxValueBackUp1 =maxValue;
            maxLoc =0;
            for (i = 0; i < strcnt;i++ )//寻找其中最长的笔画
            {
                if ((double)StrokeBoxHypLen[i] > maxValue)
                {
                    maxValue = (double)StrokeBoxHypLen[i];
                    maxLoc = i;
                }

            }
            for (m = 0; m < strcnt; m++)
            {
                if((double)StrokeBoxHypLen[m]>(0.8*maxValue))
                {
                    StrokeBoxHypLen.RemoveAt(m);//删除那条线，并插入零
                    StrokeBoxHypLen.Insert(m, (double)0);
                    max++;
                }
            }
            maxValueBackUp1 = maxValue;//用来备份最长的笔画
            maxValue = (double)StrokeBoxHypLen[0];//短的笔画,需要考虑到长宽差不多的情况。。。
            maxLoc = 0;
            for (i = 0; i < strcnt; i++)
            {
                if ((double)StrokeBoxHypLen[i] > maxValue)
                {
                    maxValue = (double)StrokeBoxHypLen[i];
                    maxLoc = i;
                }

            }

            //if(maxValueBackUp1>2*maxValue)
            //用来处理长度和宽度差不多时候的情况
            //{
            //    ;
           // }
            for (m = 0; m < strcnt; m++)
            {
                if ((double)StrokeBoxHypLen[m] > (0.8 * maxValue))
                {
                    StrokeBoxHypLen.RemoveAt(m);
                    StrokeBoxHypLen.Insert(m, (double)0);
                    mid++;
                }
            }
            maxValueBackUp2 = maxValue;
            maxValue = (double)StrokeBoxHypLen[0];
            maxLoc = 0;
            for (i = 0; i < strcnt; i++)
            {
                if ((double)StrokeBoxHypLen[i] > maxValue)
                {
                    maxValue = (double)StrokeBoxHypLen[i];
                    maxLoc = i;
                }

            }

            min = strcnt - mid - max;//文字笔画的数目



            //if ((mid <= (4 * max)) && (min >= (3 * max)))
            //{
                 for (j = 0; j < strcnt; j++)
                 {
                      if (min>0)
                      {
                          if ((double)StrokeBoxHypLen[j] > 0)
                          {
                              MyIP.Strokes[j].DrawingAttributes.Color = Colors.Brown;//文字笔画的长度设为
                          }

                       }

                  }

            // }


            


            StrokeBoxHypLen.Clear();
            textBox4.Text = strz;
        }*/

        private void buttonDivAreaStren_Click(object sender, RoutedEventArgs e)//利用区域划分来确定笔画的长度
        {
            int nN_textbook = 0;
            int i,j,k,t;
            double m, n;
            double MidVal;
            int MaxLocation;
            /*int*/ GVcnt =MyIP.Strokes.Count;
            int ptcnt;
            double MaxLengthVaule;
            double VectK;
            int MaxLengthLoc;
            string str1, str2;
            str1 = "";
            str2 = "";
            //Object ObjectMidExchange;
            ArrayList StrokeLength = new ArrayList();//记录笔画长度的链表
            ArrayList StrokeLengthBackUp = new ArrayList();//备份笔画长度
            ArrayList StrokeMaxLoc = new ArrayList();//链表用来记录最长的几笔笔画的位置
            ArrayList StrokeMaxLocBU = new ArrayList();//备份


            
            //ArrayList StrokeBoundAll = new ArrayList();//链表用来记录所有笔画的外接矩形信息
            
            ArrayList ArrayVectK = new ArrayList();//链表用来记录笔画分割时笔画的倾角信息
            ArrayList VectKDiff = new ArrayList();
            ArrayList StrokeAverage_X = new ArrayList();//用来记录表格笔画横坐标方向上的矢量，每个数据为每个笔画的平均值
            ArrayList StrokeAverage_Y = new ArrayList();


            if(textBoxnN.Text!="")
            {
                nN_textbook = Convert.ToInt32(textBoxnN.Text);
            }

            for ( i = 0; i < GVcnt; i++)
            {
                Stroke st = MyIP.Strokes[i];
                Rect Stroke_Bound = st.GetBounds();
                double Box_Hypotenuse;
                Box_Hypotenuse = Math.Sqrt(Stroke_Bound.Height * Stroke_Bound.Height + Stroke_Bound.Width * Stroke_Bound.Width);
                StrokeLength.Add(Box_Hypotenuse);
            }
            StrokeLengthBackUp = (ArrayList)StrokeLength.Clone();//浅表副本，注意和引用的区别
            if (nN_textbook <= GVcnt)//显然n的值不能超过笔画总数
            {
            for ( j = 0; j < nN_textbook; j++)
            {
                MaxLengthVaule = Convert.ToDouble(StrokeLength[j]);
                MaxLengthLoc = 0;
                for (k = 0; k < GVcnt; k++)
                {
                    if (MaxLengthVaule <=Convert.ToDouble(StrokeLength[k]))
                    {
                        MaxLengthVaule = Convert.ToDouble(StrokeLength[k]);
                        MaxLengthLoc =k;
                    }

                }
                StrokeMaxLoc.Add(MaxLengthLoc);
                StrokeLength[MaxLengthLoc] = 0;
                //ObjectMidExchange = StrokeLength[MaxLengthLoc];//这种方法在排序的时候可以用来节省空间，但是在本题中不是很适用
                //StrokeLength[MaxLengthLoc] = StrokeLength[j];
               // StrokeLength[j] = ObjectMidExchange;
                MyIP.Strokes[MaxLengthLoc].DrawingAttributes.Color = Colors.Green; 
            }}

            //对较长笔画进行分割
            
            for (i = 0; i < nN_textbook; i++)
            {
                Stroke st = MyIP.Strokes[(int)StrokeMaxLoc[i]];
                Rect Stroke_Bound = st.GetBounds();
                ptcnt = st.StylusPoints.Count;
                if (((Stroke_Bound.Height) / (double)StrokeLengthBackUp[(int)StrokeMaxLoc[i]] >= 0.2) && ((Stroke_Bound.Width) / (double)StrokeLengthBackUp[(int)StrokeMaxLoc[i]] >= 0.2))//首先是长宽比判据
                {

                    for (j = 0; j < ptcnt - 10; j++)
                    {

                        m = st.StylusPoints[j + 10].X - st.StylusPoints[j].X;
                        n = st.StylusPoints[j + 10].Y - st.StylusPoints[j].Y;
                        //strmuladd = strmuladd + " " + n/(n+t);

                        VectK = (Math.Atan(n / m)) * 180 / (Math.PI);//斜率值
                        if (m < 0)
                        {
                            VectK += 180;
                        }
                        ArrayVectK.Add(VectK);
                    }
                    for (t = 0; t < ptcnt - 11; t++)
                    {
                        VectKDiff.Add(Convert.ToDouble(ArrayVectK[t + 1]) - Convert.ToDouble(ArrayVectK[t]));//斜率向量的一阶导数
                    }
                    MidVal = Convert.ToDouble(VectKDiff[0]);
                    MaxLocation = 0;
                    for (k = 0; k < ptcnt - 11; k++)//导数的最大值
                    {
                        if (Math.Abs(Convert.ToDouble(VectKDiff[k])) > MidVal)
                        {
                            MidVal = Math.Abs(Convert.ToDouble(VectKDiff[k]));
                            MaxLocation = k;
                        }
                    }
                    MaxLocation = MaxLocation + 5;
                    //第一笔
                    StylusPoint[] MySp = new StylusPoint[MaxLocation];
                    for (int fststrk = 0; fststrk < MaxLocation; fststrk++)
                    {
                        MySp[fststrk] = new StylusPoint(st.StylusPoints[fststrk].X, st.StylusPoints[fststrk].Y);
                    }
                    StylusPointCollection MySpc = new StylusPointCollection(MySp);

                    Stroke newStroke = new Stroke(MySpc);
                    newStroke.DrawingAttributes.Color = Colors.Black;
                    //第二笔
                    StylusPoint[] MySp2 = new StylusPoint[ptcnt - MaxLocation];
                    for (int secstrk = 0; secstrk < ptcnt - MaxLocation; secstrk++)
                    {
                        MySp2[secstrk] = new StylusPoint(st.StylusPoints[MaxLocation + secstrk].X, st.StylusPoints[MaxLocation + secstrk].Y);
                    }
                    StylusPointCollection MySpc2 = new StylusPointCollection(MySp2);
                    Stroke newStroke2 = new Stroke(MySpc2);
                    newStroke2.DrawingAttributes.Color = Colors.White;



                    //MyIP.Strokes.RemoveAt((int)StrokeMaxLoc[i]);//为了避免坐标的变化，将笔画的移除操作在后面统一处理
                    //MyStrokeArrayWrite.RemoveAt((int)StrokeMaxLoc[i]);
                    MyIP.Strokes.Add(newStroke);
                    MyStrokeArrayWrite.Add(1);
                    MyIP.Strokes.Add(newStroke2);
                    MyStrokeArrayWrite.Add(1);
                    ArrayVectK.Clear();
                    VectKDiff.Clear();

                    
                }
                else
                {   //不需要分割的则直接添加笔画
                    StylusPoint[] MySp = new StylusPoint[ptcnt];
                    for (int fststrk = 0; fststrk < ptcnt; fststrk++)
                    {
                        MySp[fststrk] = new StylusPoint(st.StylusPoints[fststrk].X, st.StylusPoints[fststrk].Y);
                    }
                    StylusPointCollection MySpc = new StylusPointCollection(MySp);

                    Stroke newStrokeSingle = new Stroke(MySpc);
                    newStrokeSingle.DrawingAttributes.Color = Colors.Blue;
                    MyIP.Strokes.Add(newStrokeSingle);
                    MyStrokeArrayWrite.Add(1);
 
                }
            }
            
            
            //删除多余的笔画
            //bug有待调整
            StrokeMaxLocBU = (ArrayList)StrokeMaxLoc.Clone();
            StrokeMaxLocBU.Sort();
            StrokeMaxLocBU.Reverse();
            //考虑到数据结构，决定不采用下面这种方法
           /* for (i = 0; i < nN_textbook; i++)
            {
                Stroke st = MyIP.Strokes[(int)StrokeMaxLocBU[i]];
                Rect Stroke_Bound = st.GetBounds();
                if (((Stroke_Bound.Height) / (double)StrokeLengthBackUp[(int)StrokeMaxLocBU[i]] >= 0.2) && ((Stroke_Bound.Width) / (double)StrokeLengthBackUp[(int)StrokeMaxLocBU[i]] >= 0.2))//首先是长宽比判据
                {
                    MyIP.Strokes.RemoveAt((int)StrokeMaxLocBU[i]);//在这里做删除
                    MyStrokeArrayWrite.RemoveAt((int)StrokeMaxLocBU[i]);
                }

            }*/
            for (i = 0; i < nN_textbook; i++)
            {
                MyIP.Strokes.RemoveAt((int)StrokeMaxLocBU[i]);//在这里做删除
                MyStrokeArrayWrite.RemoveAt((int)StrokeMaxLocBU[i]);//此时表格笔画的坐标是从cnt-nN_textbook开始计算起，后面全是。注意此时cnt不等于MyIP.Strokes.Count
            }





            //判断方向，主要用来区别出纯文字
            //首先是得到方向序列

            for (i = GVcnt - nN_textbook; i < MyIP.Strokes.Count; i++)
            {
                double KAve_x=0, KAve_y=0;
                Stroke st = MyIP.Strokes[i];
                StrokeKAverage(st,ref KAve_x,ref KAve_y);
                StrokeAverage_X.Add(KAve_x);
                StrokeAverage_Y.Add(KAve_y);
                str1 = str1 + " " + KAve_x;
                str2 = str2 + " " + KAve_y; 
            }
            //文本框显示
           // textBoxAverageX.Text = str1;
          //  textBoxAverageY.Text = str2;
            
            //排除纯文字的类型
            i = 0;
            j = 0;
            for (i = 0; i < (MyIP.Strokes.Count - GVcnt + nN_textbook); i++)
            {
                if ((Math.Abs((double)StrokeAverage_X[i]) < 10) || (Math.Abs((double)StrokeAverage_Y[i]) < 10))
                    j=j+1;
            }
            string strpureword;
            strpureword ="0" ;

            if (j != (MyIP.Strokes.Count - GVcnt + nN_textbook))
            {
                strpureword ="1";
            }
            textBoxPureWord.Text =strpureword;
            PureWord = 1;

            if (strpureword == "0")//只有当不是纯文字的时候才进行处理
            {
                PureWord = 0;

                //确定每个笔画的方向，水平或者竖直
                //水平为1，竖直为0
                string strdirection;
                strdirection = "";
                //下面这一句不知道要不要啊
                StrokeDirectMax.Clear();
                for (i = 0; i < (MyIP.Strokes.Count - GVcnt + nN_textbook); i++)
                {
                    if ((Math.Abs((double)StrokeAverage_Y[i]) < 10))
                    {
                        StrokeDirectMax.Add(1);
                    }
                    else if (Math.Abs((double)StrokeAverage_X[i]) < 10)
                    {
                        StrokeDirectMax.Add(0);
                    }
                    strdirection = strdirection + " " + StrokeDirectMax[i];
                }
              //  textBoxDirection.Text = strdirection;



                //确定每个笔画的外接矩形，采用近似方法获取长度
                for (i = 0; i < (MyIP.Strokes.Count - GVcnt + nN_textbook); i++)
                {
                    //Stroke st = MyIP.Strokes[i];
                    Stroke st = MyIP.Strokes[i + GVcnt - nN_textbook];//需要注意这一点
                    Rect MyStrokeBound = st.GetBounds();
                    //StrokeBoundMax.Add(MyStrokeBound);
                    StrokeBoundX.Add(MyStrokeBound.X);
                    StrokeBoundY.Add(MyStrokeBound.Y);
                    if ((int)StrokeDirectMax[i] == 1)
                    {
                        StrokeLengthMax.Add(MyStrokeBound.Width);
                    }
                    else if ((int)StrokeDirectMax[i] == 0)
                    {
                        StrokeLengthMax.Add(MyStrokeBound.Height);
                    }
                    else
                    { }
                }







                //利用外接矩形对笔画进行分组，
                //分组第一步，求取水平线最长、最短的长度 与竖直线最短最长的长度

                maxHorizonLen = 0;
                minHorizonLen = 10000;
                maxVerticalLen = 0;
                minVerticalLen = 10000;
                for (i = 0; i < (MyIP.Strokes.Count - GVcnt + nN_textbook); i++)
                {
                    if ((int)StrokeDirectMax[i] == 1)
                    {
                        //StrokeLengthMax.Add(MyStrokeBound.Width);
                        if (maxHorizonLen < (double)StrokeLengthMax[i])
                        {
                            maxHorizonLen = (double)StrokeLengthMax[i];
                        }
                        if (minHorizonLen > (double)StrokeLengthMax[i])
                        {
                            minHorizonLen = (double)StrokeLengthMax[i];
                        }
                    }
                    else if ((int)StrokeDirectMax[i] == 0)
                    {
                        //StrokeLengthMax.Add(MyStrokeBound.Height);
                        if (maxVerticalLen < (double)StrokeLengthMax[i])
                        {
                            maxVerticalLen = (double)StrokeLengthMax[i];

                        }
                        if (minVerticalLen > (double)StrokeLengthMax[i])
                        {
                            minVerticalLen = (double)StrokeLengthMax[i];

                        }
                    }
                    else
                    { }

                }

                //分组第二步，求取距离
                //采用的是调用两个笔画之间的距离函数

                //测试用例i，j
                i = 0; j = 1;

                double deltaX, deltaY;
                deltaX = 0;
                deltaY = 0;
                DistanceTwoStrokes(ref deltaX, ref deltaY, i, j);

                //测试显示
              //  textBoxdeltaX.Text = Convert.ToString(deltaX);
              //  textBoxdeltaY.Text = Convert.ToString(deltaY);


                //由距离确定联通的笔画
                StrokeCollection myStrokesClet = new StrokeCollection();
                for (i = GVcnt - nN_textbook; i < MyIP.Strokes.Count; i++)
                {
                    myStrokesClet.Add(MyIP.Strokes[i]);
                }

               
                AreaResult = segmentStrokes(myStrokesClet);

                //由笔画找出联通的大区域
                int AreaCnt = AreaResult.Count;
                int PreAreaCnt = 0;
                double[] max_X = new double[AreaCnt];
                double[] max_Y = new double[AreaCnt];
                double[] min_X = new double[AreaCnt];
                double[] min_Y = new double[AreaCnt];
                //max_X = min_X = MyIP.Strokes[cnt - nN_textbook].GetBounds().X;
                //max_Y = min_Y = MyIP.Strokes[cnt - nN_textbook].GetBounds().Y;
                for (i = 0; i < AreaCnt; i++)
                {
                    max_X[i] = max_Y[i] = 0;//initial
                    min_X[i] = min_Y[i] = 10000;
                    PreAreaCnt = AreaResult[i].Count;
                    for (j = 0; j < PreAreaCnt; j++)//find max and min 
                    {
                        Stroke st = MyIP.Strokes[AreaResult[i][j] + GVcnt - nN_textbook];
                        if (min_X[i] > st.GetBounds().Left)
                        {
                            min_X[i] = st.GetBounds().Left;
                        }
                        if (max_X[i] < st.GetBounds().Right)
                        {
                            max_X[i] = st.GetBounds().Right;
                        }
                        if (min_Y[i] > st.GetBounds().Top)
                        {
                            min_Y[i] = st.GetBounds().Top;

                        }
                        if (max_Y[i] < st.GetBounds().Bottom)
                        {
                            max_Y[i] = st.GetBounds().Bottom;
                        }

                    }
                    min_X[i] = min_X[i] * 0.8;// expand the area
                    min_Y[i] = min_Y[i] * 0.8;
                    max_X[i] = max_X[i] * 1.2;
                    max_Y[i] = max_Y[i] * 1.2;



                }


                for (i = 0; i < AreaCnt; i++)//Area
                {
                    double AreaThreshold = new double();
                    PreAreaCnt = AreaResult[i].Count;
                    AreaThreshold = 10000;
                    for (j = 0; j < PreAreaCnt; j++)
                    {
                        if (AreaThreshold > (double)StrokeLengthMax[AreaResult[i][j]])
                            AreaThreshold = (double)StrokeLengthMax[AreaResult[i][j]];
                    }

                    //遍历笔画判断是否在这一区域，
                    for (j = 0; j < GVcnt - nN_textbook; j++)
                    {
                        int _m, _n, _s, _t;
                        int liantong;
                        liantong = 0;
                        Stroke st = MyIP.Strokes[j];
                        Rect stRect = st.GetBounds();
                        double Stroke_Length;
                        _m = (min_X[i] < stRect.Left) ? 1 : 0;
                        _n = (max_X[i] > stRect.Right) ? 1 : 0;
                        _s = (min_Y[i] < stRect.Top) ? 1 : 0;
                        _t = (max_Y[i] > stRect.Bottom) ? 1 : 0;
                        if (((_m + _n + _s + _t) == 4)/*||(_m+_s==2)||(_n+_t==2)*/)//belong to this area
                        {

                            //if (Stroke_Length < 0.5 * AreaThreshold)
                            //{
                            //   st.DrawingAttributes.Color = Colors.Green;
                            //}
                            //笔画需要联通，方向、长度三个角度综合考考虑
                           // for (t = 0; t < AreaResult.Count; t++)
                           // {
                                //PreAreaCnt = AreaResult[i].Count;
                            for (k = 0; k < AreaResult[i].Count; k++)
                                {
                                    if (strokeConnectionCheck(MyIP.Strokes, j, AreaResult[i][k] + GVcnt - nN_textbook))//连通
                                    {
                                       liantong ++;
                                    
                                }
                           // }



                        }
                            if(liantong!=0)
                            {
                                double kx, ky;
                                kx = ky = 0;
                                StrokeKAverage(st, ref kx, ref ky);
                                if ((Math.Abs(kx) < 10) || ((Math.Abs(ky) < 10)))//方向
                                {
                                    Stroke_Length = Math.Sqrt(stRect.Height * stRect.Height + stRect.Width * stRect.Width);
                                    if (Stroke_Length > 0.4 * AreaThreshold)//阈值
                                    {
                                        //表格笔画则复杂一点
                                        int myptcnt = MyIP.Strokes[j].StylusPoints.Count;
                                        StylusPoint[] MySp = new StylusPoint[myptcnt];
                                        for (int fststrk = 0; fststrk < myptcnt; fststrk++)
                                        {
                                            MySp[fststrk] = new StylusPoint(st.StylusPoints[fststrk].X, st.StylusPoints[fststrk].Y);
                                        }
                                        StylusPointCollection MySpc = new StylusPointCollection(MySp);

                                        Stroke newStroke = new Stroke(MySpc);
                                        MyIP.Strokes.RemoveAt(j);
                                        MyStrokeArrayWrite.RemoveAt(j);
                                        MyIP.Strokes.Add(newStroke);
                                        MyStrokeArrayWrite.Add(1);
                                        if ((Math.Abs(kx) < 10))
                                        {
                                            StrokeDirectMax.Add(0);

                                        }
                                        else
                                        {
                                            StrokeDirectMax.Add(1);
                                        }
                                        

                                        GVcnt = GVcnt - 1;
                                        j = j - 1;
                                        //将该笔画加入该区域
                                        AreaResult[i].Add(MyIP.Strokes.Count - 1 - GVcnt + nN_textbook);

                                        MyIP.Strokes[MyIP.Strokes.Count - 1].DrawingAttributes.Color = Colors.Gray;//表格笔画
                                        // st.DrawingAttributes.Color = Colors.Gray;

                                    }
                                    else
                                        st.DrawingAttributes.Color = Colors.Green;//文字笔画

                                }
                                else
                                    st.DrawingAttributes.Color = Colors.Green;
                                //  break;

                            }
                            else
                                st.DrawingAttributes.Color = Colors.Green;
                            }
                        //else
                        //    st.DrawingAttributes.Color = Colors.Green;

                    }

                    


                }
                //所有区域外的笔画
                for (i = 0; i < GVcnt - nN_textbook; i++)
                {
                    Stroke st = MyIP.Strokes[i];
                    if ((st.DrawingAttributes.Color != Colors.Gray) && (st.DrawingAttributes.Color != Colors.Green))
                    {
                        st.DrawingAttributes.Color = Colors.PaleGoldenrod;
                    }
                }






                //在区域内，对表格和文字进行分组




            }
            /*else
            {
                if((bool)radioButton1.IsChecked)
                {
                    //System.Threading.Thread.Sleep(5000);

                    for (i = 0; i <MyIP.Strokes.Count; i++)
                    {
                        Stroke st = MyIP.Strokes[i];
                        st.DrawingAttributes.Color = Colors.White;

                    }

                }
            }*/

            GVcnt = GVcnt - nN_textbook;


            
            







            StrokeBoundX.Clear();
            StrokeBoundY.Clear();
           // StrokeDirectMax.Clear();
            StrokeAverage_X.Clear();//释放空间
            StrokeAverage_Y.Clear();
            StrokeMaxLoc.Clear();
            StrokeBoundMax.Clear();
            StrokeLengthMax.Clear();
           // StrokeBoundAll.Clear();
            StrokeLengthBackUp.Clear();
            StrokeLength.Clear();
            nN_textbook = 0;//

            //统一显示效果

            buttonDivDisplay_Click(sender, e);






        }

        
        //统一效果显示
        private void buttonDivDisplay_Click(object sender, RoutedEventArgs e)
        {
            int k=0;
            for (int i = 0; i < MyIP.Strokes.Count; i++)
            {
                Stroke st = MyIP.Strokes[i];
               
                if (st.DrawingAttributes.Color == Colors.PaleGoldenrod || st.DrawingAttributes.Color == Colors.Green)
                {
                    k++;
                }
               
            }
            for (int i = 0; i < MyIP.Strokes.Count; i++)
            {
                Stroke st = MyIP.Strokes[i];
                if (PureWord == 0)
                {
                    
                    if (k != 0)
                    {


                        if (st.DrawingAttributes.Color == Colors.PaleGoldenrod || st.DrawingAttributes.Color == Colors.Green)
                        {
                            st.DrawingAttributes.Color = Colors.White;
                        }
                        else
                            st.DrawingAttributes.Color = Colors.Yellow;


                    }
                    else 
                    {
                        st.DrawingAttributes.Color = Colors.Yellow;

                    }
                    
                }
                else
                    st.DrawingAttributes.Color = Colors.White;
            }

        }



        //求斜率平均值的函数，输入为笔画Stroke类型，输出为斜率的平均值
        public void StrokeKAverage(Stroke st,ref double KAve_x,ref double KAve_y)
        {
            int ptcnt;
            double  m, n;
            double mysumx,mysumy;
            mysumx = 0;
            mysumy = 0;
            ptcnt = st.StylusPoints.Count;
            ArrayList ArrayVectKx = new ArrayList();
            ArrayList ArrayVectKy = new ArrayList();
            for (int k = 0; k < ptcnt - 10; k++)
            {
                m = st.StylusPoints[k + 10].X - st.StylusPoints[k].X;
                n = st.StylusPoints[k + 10].Y - st.StylusPoints[k].Y;
                /*
                vectK = (Math.Atan(n / m)) * 180 / (Math.PI);//斜率值计算量貌似有一点儿大
                if (m < 0)
                {
                    vectK += 180;
                }
                ArrayVectK.Add(vectK);
                */
                mysumx += m;
                mysumy += n;
            }
            KAve_x = mysumx / (ptcnt);
            KAve_y = mysumy / (ptcnt - 10);

            /*
            for (i = 0; i < ptcnt;i++ )
            {
                mysum += (double)ArrayVectK[i];
            }*/
            //ArrayVectK.Clear();
            //return (mysum/ptcnt);
        }

        //该函数的作用是求两个笔画之间的距离 输入为两个笔画的索引，输出为deltaX和deltaY
        public void DistanceTwoStrokes(ref  double deltaX,ref  double deltaY,int i,int j)
        {
            int Di, Dj;
            double X1, X1L, X0, Y1, Y0, Y0L;
            Di = 0; Dj = 0;
            X1 = 0; Y1 = 0; X1L = 0;
            X0 = 0; Y0 = 0; Y0L = 0;

            deltaX = 0; deltaY = 0;
            if ((int)StrokeDirectMax[i] == 1)
            { Di = 1; }
            else if ((int)StrokeDirectMax[i] == 0)
            { Di = 0; }
            if ((int)StrokeDirectMax[j] == 1)
            { Dj = 1; }
            else if ((int)StrokeDirectMax[j] == 0)
            { Dj = 0; }

            if ((Di + Dj) == 1)
            {
                if (Di == 1)
                {
                    X1 = (double)StrokeBoundX[i];
                    Y1 = (double)StrokeBoundY[i];
                    X1L = (double)StrokeBoundX[i] + (double)StrokeLengthMax[i];
                    X0 = (double)StrokeBoundX[j];
                    Y0 = (double)StrokeBoundY[j];
                    Y0L = (double)StrokeBoundY[i] + (double)StrokeLengthMax[j];
                }
                else
                {
                    X1 = (double)StrokeBoundX[j];
                    Y1 = (double)StrokeBoundY[j];
                    X1L = (double)StrokeBoundX[j] + (double)StrokeLengthMax[j];
                    X0 = (double)StrokeBoundX[i];
                    Y0 = (double)StrokeBoundY[i];
                    Y0L = (double)StrokeBoundY[i] + (double)StrokeLengthMax[i];
                }
                if (X0 < X1)
                {
                    deltaX = X1 - X0;
                }
                else if (X0 > X1L)
                {
                    deltaX = X0 - X1L;
                }
                else
                {
                    deltaX = 0;
                }
                if (Y1 < Y0)
                {
                    deltaY = Y0 - Y1;
                }
                else if (Y1 > Y0L)
                {
                    deltaY = Y1 - Y0L;
                }
                else
                {
                    deltaY = 0;
                }

            }
            else if ((Di + Dj) == 2)
            {

                deltaY = ((double)StrokeBoundY[i] > (double)StrokeBoundY[j]) ? ((double)StrokeBoundY[i] - (double)StrokeBoundY[j]) : ((double)StrokeBoundY[j] - (double)StrokeBoundY[i]);
                if (((double)StrokeBoundX[j] > (double)StrokeBoundX[i] + (double)StrokeLengthMax[i]) || ((double)StrokeBoundX[i] > (double)StrokeBoundX[j] + (double)StrokeLengthMax[j]))
                {
                    deltaX = ((double)StrokeBoundX[j] > (double)StrokeBoundX[i] + (double)StrokeLengthMax[i]) ? ((double)StrokeBoundX[j] - (double)StrokeBoundX[i] - (double)StrokeLengthMax[i]) : ((double)StrokeBoundX[i] - (double)StrokeBoundX[j] - (double)StrokeLengthMax[j]);
                }
                else
                    deltaX = 0;
            }
            else
            {
                deltaX = ((double)StrokeBoundX[i] > (double)StrokeBoundX[j]) ? ((double)StrokeBoundX[i] - (double)StrokeBoundX[j]) : ((double)StrokeBoundX[j] - (double)StrokeBoundX[i]);
                if (((double)StrokeBoundY[j] > (double)StrokeBoundY[i] + (double)StrokeLengthMax[i]) || ((double)StrokeBoundY[i] > (double)StrokeBoundY[j] + (double)StrokeLengthMax[j]))
                {
                    deltaY = ((double)StrokeBoundY[j] > (double)StrokeBoundY[i] + (double)StrokeLengthMax[i]) ? ((double)StrokeBoundY[j] - (double)StrokeBoundY[i] - (double)StrokeLengthMax[i]) : ((double)StrokeBoundY[i] - (double)StrokeBoundY[j] - (double)StrokeLengthMax[j]);
                }
                else
                    deltaY = 0;
            }
           

            
        }
        public /*static*/ Collection<Collection<Int32>> GroupPoints(Collection<Point> rawPoints,int number)
        {
            Collection<Int32> idxs = new Collection<int>();
            Collection<Collection<Int32>> result = new Collection<Collection<int>>();
            int nclasses = 0;
            int i, j, N;
            for (i = 0; i < rawPoints.Count; i++)
                idxs.Add(i);
            N = idxs.Count;
            int[] label = new int[N];
            if (N == 0)
                return result;
            ;
            const int PARENT = 0;
            const int RANK = 1;
            int[,] _nodes = new int[N, 2];
            for (i = 0; i < N; i++)
            {
                _nodes[i, PARENT] = -1;
                _nodes[i, RANK] = 0;
            }
            for (i = 0; i < N; i++)
            {
                int root = i;
                // find root
                while (_nodes[root, PARENT] >= 0)
                    root = _nodes[root, PARENT];
                for (j = 0; j < N; j++)
                {
                    if (i == j || !InnerPointCheck(rawPoints, idxs[i], idxs[j],number))
                        continue;
                    int root2 = j;
                    while (_nodes[root2, PARENT] >= 0)
                        root2 = _nodes[root2, PARENT];
                    if (root2 != root)
                    {
                        // unite both trees
                        int rank = _nodes[root, RANK], rank2 = _nodes[root2, RANK];
                        if (rank > rank2)
                            _nodes[root2, PARENT] = root;
                        else
                        {
                            _nodes[root, PARENT] = root2;
                            if (rank == rank2)
                                _nodes[root2, RANK]++;
                            root = root2;
                        }
                        //assert( nodes[root][PARENT] < 0 );
                        int k = j, parent;
                        // compress the path from node2 to root
                        while ((parent = _nodes[k, PARENT]) >= 0)
                        {
                            _nodes[k, PARENT] = root;
                            k = parent;
                        }
                        // compress the path from node to root
                        k = i;
                        while ((parent = _nodes[k, PARENT]) >= 0)
                        {
                            _nodes[k, PARENT] = root;
                            k = parent;
                        }
                    }
                }

            }
            // Final O(N) pass: enumerate classes
            for (i = 0; i < N; i++)
            {
                int root = i;
                while (_nodes[root, PARENT] >= 0)
                    root = _nodes[root, PARENT];
                // re-use the rank as the class label
                if (_nodes[root, RANK] >= 0)
                    _nodes[root, RANK] = ~nclasses++;
                label[i] = ~_nodes[root, RANK];
            }
            ////////////////////
            Collection<int> classLabel = new Collection<int>();
            for (int t = 0; t < N; t++)
            {
                bool isFind = false;
                for (int tt = 0; tt < classLabel.Count; tt++)
                {
                    if (label[t] == classLabel[tt])
                    {
                        isFind = true;
                        break;
                    }
                }
                if (isFind == false)
                    classLabel.Add(label[t]);
            }
            for (int t = 0; t < nclasses; t++)
            {
                Collection<int> _ttemp = new Collection<int>();
                for (int tt = 0; tt < N; tt++)
                {
                    if (classLabel[t] == label[tt])
                    {
                        _ttemp.Add(idxs[tt]);
                    }
                }
                result.Add(_ttemp);
            }
            return result;
        }
        private /*static*/ bool InnerPointCheck(Collection<Point> rawPoints, int idx1, int idx2,int number)
        {
            Point p1 = rawPoints[idx1];
            Point p2 = rawPoints[idx2];

            /* if (p1.X != p2.X && p1.Y != p2.Y)
                 return false;
             if (p1.X == p2.X)
             {
                 for (int i = 0; i < horLines.count(); i++)
                 {
                     if (horLines[i].left <= p1.X && horLines[i].right >= p1.X && )
                        
                 }
             }
             else if (p1.Y == p2.Y)
             {

             }*/
            int j, k, m, n;
            j = k = 0;
            m = n = 0;
            for (int i = 0; i < AreaResult[number].Count; i++)
            {

                Stroke st = MyIP.Strokes[AreaResult[number][i] + GVcnt];
                if (p1.X != p2.X && p1.Y != p2.Y)
                    return false;

                if ((int)StrokeDirectMax[AreaResult[number][i]] == 1)
                {
                    if (p1.X == p2.X)
                    {
                        Point ptpt1 = ((Point[])StrokeLinePt[i])[0];
                        Point ptpt2 = ((Point[])StrokeLinePt[i])[1];
                        if ((p1.Y - ptpt1.Y) * (ptpt1.Y - p2.Y) > 0)
                        {
                            m++;
                            if ((p1.X > ptpt2.X || p1.X < ptpt1.X))
                            {
                                // return true;
                                j++;
                            }


                        }

                    }
                }

                if ((int)StrokeDirectMax[AreaResult[number][i]] == 0)
                {
                    if (p1.Y == p2.Y)
                    {
                        Point ptpt1 = ((Point[])StrokeLinePt[i])[0];
                        Point ptpt2 = ((Point[])StrokeLinePt[i])[1];
                        if ((p1.X - ptpt1.X) * (ptpt1.X - p2.X) > 0)
                        {
                            n++;
                            if (p1.Y > ptpt2.Y || p1.Y < ptpt1.Y)
                            {
                                k++;
                            }
                        }


                    }

                }
            }
            if ((m == 1 && j == 1) || (n == 1 && k == 1))
            {
                return true;
            }
            return false;

        }
        /***********
[参数] StrokeCollection strokes
[返回]二维Collection, //值为笔画标号
示例：
假如输入的有十个笔画，
返回的Collection<Collection<Int32>>为
{
	{0， 3， 8}
	{1， 2， 4， 5， 6， 9}
	{7}
}
表示有三组，
***********/

        public /*static*/ Collection<Collection<Int32>> segmentStrokes(StrokeCollection strokes)
        {
            Collection<Int32> idxs = new Collection<int>();
            Collection<Collection<Int32>> result = new Collection<Collection<int>>();
            int nclasses = 0;
            int i, j, N;
            for (i = 0; i < strokes.Count; i++)
                idxs.Add(i);
            N = idxs.Count;
            int[] label = new int[N];
            if (N == 0)
                return result;
           ;
            const int PARENT = 0;
            const int RANK = 1;
            int[,] _nodes = new int[N, 2];
            for (i = 0; i < N; i++)
            {
                _nodes[i, PARENT] = -1;
                _nodes[i, RANK] = 0;
            }
            for (i = 0; i < N; i++)
            {
                int root = i;
                // find root
                while (_nodes[root, PARENT] >= 0)
                    root = _nodes[root, PARENT];
                for (j = 0; j < N; j++)
                {
                    if (i == j || !strokeConnectionCheck(strokes, idxs[i], idxs[j]))
                        continue;
                    int root2 = j;
                    while (_nodes[root2, PARENT] >= 0)
                        root2 = _nodes[root2, PARENT];
                    if (root2 != root)
                    {
                        // unite both trees
                        int rank = _nodes[root, RANK], rank2 = _nodes[root2, RANK];
                        if (rank > rank2)
                            _nodes[root2, PARENT] = root;
                        else
                        {
                            _nodes[root, PARENT] = root2;
                            if (rank == rank2)
                                _nodes[root2, RANK]++;
                            root = root2;
                        }
                        //assert( nodes[root][PARENT] < 0 );
                        int k = j, parent;
                        // compress the path from node2 to root
                        while ((parent = _nodes[k, PARENT]) >= 0)
                        {
                            _nodes[k, PARENT] = root;
                            k = parent;
                        }
                        // compress the path from node to root
                        k = i;
                        while ((parent = _nodes[k, PARENT]) >= 0)
                        {
                            _nodes[k, PARENT] = root;
                            k = parent;
                        }
                    }
                }

            }
            // Final O(N) pass: enumerate classes
            for (i = 0; i < N; i++)
            {
                int root = i;
                while (_nodes[root, PARENT] >= 0)
                    root = _nodes[root, PARENT];
                // re-use the rank as the class label
                if (_nodes[root, RANK] >= 0)
                    _nodes[root, RANK] = ~nclasses++;
                label[i] = ~_nodes[root, RANK];
            }
            ////////////////////
            Collection<int> classLabel = new Collection<int>();
            for (int t = 0; t < N; t++)
            {
                bool isFind = false;
                for (int tt = 0; tt < classLabel.Count; tt++)
                {
                    if (label[t] == classLabel[tt])
                    {
                        isFind = true;
                        break;
                    }
                }
                if (isFind == false)
                    classLabel.Add(label[t]);
            }
            for (int t = 0; t < nclasses; t++)
            {
                Collection<int> _ttemp = new Collection<int>();
                for (int tt = 0; tt < N; tt++)
                {
                    if (classLabel[t] == label[tt])
                    {
                        _ttemp.Add(idxs[tt]);
                    }
                }
                result.Add(_ttemp);
            }
            return result;
        }
        private /*static*/ bool strokeConnectionCheck(StrokeCollection strokes, int idx1, int idx2)
        {
            Rect _rect1 = strokes[idx1].GetBounds();
            Rect _rect2 = strokes[idx2].GetBounds();
            double delta_x = 0, delta_y = 0;
            delta_x = Math.Max(_rect1.Left, _rect2.Left) - Math.Min(_rect1.Right, _rect2.Right);
            if (delta_x < 0) delta_x = 0;
            delta_y = Math.Max(_rect1.Top, _rect2.Top) - Math.Min(_rect1.Bottom, _rect2.Bottom);
            if (delta_y < 0) delta_y = 0;
            double dis = Math.Sqrt(delta_x * delta_x + delta_y * delta_y);
            
           // if (dis < 100)
            //    return true;
            //由于联通的相互性，其实必要性不大
           /* int m, n;
            m = (int)StrokeDirectMax[idx1];
            n = (int)StrokeDirectMax[idx2];
            if (((m == 1) && (n == 1)) || ((m == 0) && (n == 0)))
            {
                if ((dis < 1.2 * maxHorizonLen) || (dis < 1.2 * maxVerticalLen))
                {
                    return true;
                }
                return false;
            }
            else
            {
                if ((dis < 0.3 * minHorizonLen) && (dis < 0.3 * minVerticalLen))
                {
                    return true;
                }
                return false;
            }*/
                
            
           // return false;
           if ((dis < 0.2 * minHorizonLen) && (dis < 0.2 * minVerticalLen))
           {
                return true;
            }
            return false;
        }

        private void buttonFormAreaLoc_Click(object sender, RoutedEventArgs e)
        {
            //int cnt =MyIP.Strokes.Count;
            int LineHCnt,LineVCnt;
            LineHCnt = LineVCnt =1;
             //int InOutFlag;

           // if (GVcnt == 0)
           // {
           //     return;
           // }

            MyIP.Children.Clear();
            for (int m = 0; m < AreaResult.Count; m++)
            {
                for (int i = 0; i < AreaResult[m].Count; i++)//表格笔画
                {
                    Stroke st = MyIP.Strokes[AreaResult[m][i] + GVcnt];
                    // st.DrawingAttributes.Color = Colors.Red;

                    //第一步是求取拟合直线
                    if ((int)StrokeDirectMax[AreaResult[m][i]] == 1)
                    {
                        //int ptcnt = st.StylusPoints.Count;
                        //double dTemp = 0;
                        //for (int j = 0; j < ptcnt; j++)
                        //{
                        //    dTemp = dTemp + st.StylusPoints[j].Y;
                        //}
                        //dTemp = dTemp / ptcnt;
                        double dTemp = 0;
                        dTemp = st.GetBounds().Y + st.GetBounds().Height * 0.5;
                        StrokeLineH.Add(dTemp);

                    }
                    else
                    {
                        //int ptcnt = st.StylusPoints.Count;
                        //double dTemp = 0;
                        // for (int j = 0; j < ptcnt; j++)
                        //{
                        //    dTemp = dTemp + st.StylusPoints[j].X;
                        // }
                        // dTemp = dTemp / ptcnt;
                        double dTemp = 0;
                        dTemp = st.GetBounds().X + st.GetBounds().Width * 0.5;
                        StrokeLineV.Add(dTemp);
                    }
                }
                //第二步求取单元表格及其中心点
                LineHCnt = StrokeLineH.Count - 1;
                LineVCnt = StrokeLineV.Count - 1;
                StrokeLineH.Sort();
                StrokeLineV.Sort();
                for (int i = 0; i < LineHCnt; i++)
                {
                    for (int j = 0; j < LineVCnt; j++)
                    {
                        Point pt = new Point();
                        pt.X = ((double)StrokeLineV[j] + (double)StrokeLineV[j + 1]) * 0.5;
                        pt.Y = ((double)StrokeLineH[i] + (double)StrokeLineH[i + 1]) * 0.5;
                        CenterPoint.Add(pt);
                        //((Point)CenterPoint[0]).;
                    }
                }

                //确定每个笔画的端点坐标

                for (int i = 0; i < AreaResult[m].Count; i++)
                {
                    Point[] Linept = new Point[2];
                    Stroke st = MyIP.Strokes[AreaResult[m][i] + GVcnt];
                    if ((int)StrokeDirectMax[AreaResult[m][i]] == 1)
                    {

                        Linept[0].Y = Linept[1].Y = st.GetBounds().Y + st.GetBounds().Height * 0.5;
                        Linept[0].X = st.GetBounds().X;
                        Linept[1].X = st.GetBounds().X + st.GetBounds().Width;
                        StrokeLinePt.Add(Linept);
                    }
                    else
                    {
                        //Point[] Linept = new Point[2];
                        Linept[0].X = Linept[1].X = st.GetBounds().X + st.GetBounds().Width * 0.5;
                        Linept[0].Y = st.GetBounds().Y;
                        Linept[1].Y = st.GetBounds().Y + st.GetBounds().Height;
                        StrokeLinePt.Add(Linept);
                    }

                }

                //判断内点与外点，形成内点集合
                int[] LeiXing = new int[LineHCnt * LineVCnt];
                Collection<Point> InPoints = new Collection<Point>();
                int mm, n, s, t;
                mm = n = s = t = 0;
                Collection<int> InPointLoc = new Collection<int>();
                //int InPtCnt=0;
                for (int i = 0; i < LineHCnt * LineVCnt; i++)//每个点
                {
                    LeiXing[i] = 0;
                    Point ptpt = (Point)CenterPoint[i];
                    for (int j = 0; j < AreaResult[m].Count; j++)
                    {
                        Point ptpt1 = ((Point[])StrokeLinePt[j])[0];
                        Point ptpt2 = ((Point[])StrokeLinePt[j])[1];
                        if ((int)StrokeDirectMax[AreaResult[m][j]] == 1)
                        {
                            if (ptpt.Y < ptpt1.Y)
                            {
                                if (ptpt.X < ptpt2.X && ptpt.X > ptpt1.X)
                                {
                                    mm++;
                                }

                            }
                            else
                            {
                                if (ptpt.X < ptpt2.X && ptpt.X > ptpt1.X)
                                {
                                    n++;
                                }

                            }
                        }
                        if ((int)StrokeDirectMax[AreaResult[m][j]] == 0)
                        {
                            if (ptpt.X < ptpt1.X)
                            {
                                if (ptpt.Y < ptpt2.Y && ptpt.Y > ptpt1.Y)
                                {
                                    s++;
                                }

                            }
                            else
                            {
                                if (ptpt.Y < ptpt2.Y && ptpt.Y > ptpt1.Y)
                                {
                                    t++;
                                }

                            }

                        }
                    }
                    if (mm > 0 && n > 0 && s > 0 && t > 0)
                    {
                        LeiXing[i] = 1;
                    }
                    mm = n = s = t = 0;
                    if (LeiXing[i] == 1)
                    {
                        InPoints.Add(ptpt);

                        InPointLoc.Add(i);


                    }
                    //InPtCnt++;

                }

                //判断内点之间的区域划分关系
                Collection<Collection<Int32>> AreaInPoint = new Collection<Collection<int>>();
                AreaInPoint = GroupPoints(InPoints, m);

                //由内点确定相应区域


                for (int strokeK = 0; strokeK < GVcnt; strokeK++)
                {
                    Stroke st = MyIP.Strokes[strokeK];
                    double st_minY, st_minX, st_maxY, st_maxX;
                    st_minY = st.GetBounds().Top;
                    st_minX = st.GetBounds().Left;
                    st_maxY = st.GetBounds().Bottom;
                    st_maxX = st.GetBounds().Right;




                    //首先填充文字笔画
                    for (int inm = 0; inm < AreaInPoint.Count; inm++)//每组内点
                    {
                        double maxX, minX, maxY, minY;
                        int kkk = 0;
                        byte r = 0;

                        for (int inn = 0; inn < AreaInPoint[inm].Count; inn++)//每个内点
                        {
                            int HangShu = ((int)InPointLoc[AreaInPoint[inm][inn]]) / LineVCnt;
                            int LieShu = ((int)InPointLoc[AreaInPoint[inm][inn]]) % LineVCnt;
                            minY = (double)(StrokeLineH[HangShu]);
                            minX = (double)(StrokeLineV[LieShu]);
                            maxY = (double)(StrokeLineH[HangShu + 1]);
                            maxX = (double)(StrokeLineV[LieShu + 1]);
                            if ((st_minY > minY) && (st_minX > minX) && (st_maxY < maxY) && (st_maxX < maxX))
                            {
                                kkk++;


                            }
                            



                        }
                        if (kkk > 0)
                        {
                            /*if (inm == 0)
                            {
                                st.DrawingAttributes.Color = Colors.Blue;
                            }
                            else if (inm == 1)
                            {
                                st.DrawingAttributes.Color = Colors.Black;
                            }
                            else
                            {
                                st.DrawingAttributes.Color = Colors.HotPink;
                            }*/
                            //System.Random r = new System.Random();
                            //System.Random g = new System.Random();
                            //System.Random b = new System.Random();
                            r = (byte)(r + 60 * inm);
                            st.DrawingAttributes.Color = Color.FromArgb(255, r, (byte)(r + 100), (byte)(r + 200));
                            int cntForm = 0;
                            int cntArea = 0;
                            if ((textBoxFormNum.Text != "") || (textBox7AreaNum.Text != ""))
                            {
                                cntForm = Convert.ToInt32(textBoxFormNum.Text) - 1;
                                cntArea = Convert.ToInt32(textBox7AreaNum.Text) - 1;
                                if ((cntForm == m) && (cntArea == inm))
                                {
                                    st.DrawingAttributes.Color = Colors.MediumPurple;

                                }
                            }
                        }
                    }

                    
                    

                }
                //其次填充所对应矩形区域
                byte myr = 40;
                for (int inm = 0; inm < AreaInPoint.Count; inm++)//每组内点
                {
                    double maxX, minX, maxY, minY;
                    //int kkk = 0;
                    myr = (byte)(myr + 60 * inm);
                    Color myclr = Color.FromArgb(255, myr, (byte)(myr + 100), (byte)(myr +200 ));

                    for (int inn = 0; inn < AreaInPoint[inm].Count; inn++)//每个内点
                    {
                        int HangShu = ((int)InPointLoc[AreaInPoint[inm][inn]]) / LineVCnt;
                        int LieShu = ((int)InPointLoc[AreaInPoint[inm][inn]]) % LineVCnt;
                        minY = (double)(StrokeLineH[HangShu]);
                        minX = (double)(StrokeLineV[LieShu]);
                        maxY = (double)(StrokeLineH[HangShu + 1]);
                        maxX = (double)(StrokeLineV[LieShu + 1]);
                        

                        Rectangle myrect = new Rectangle();
                        myrect.Fill = new SolidColorBrush(myclr);
                        myrect.Opacity = 0.3;
                        //myrect.Stroke = new SolidColorBrush(Colors.Gray);
                        myrect.Width = maxX - minX;
                        myrect.Height = maxY - minY;
                        MyIP.Children.Add(myrect);
                        InkCanvas.SetLeft(myrect, minX);
                        InkCanvas.SetTop(myrect, minY);


                        // }
                    }
                }

                //每个区域的临时数据需要删除
                InPointLoc.Clear();
                InPoints.Clear();
                StrokeLinePt.Clear();
                CenterPoint.Clear();
                StrokeLineH.Clear();
                StrokeLineV.Clear();
            }



















            AreaResult.Clear();
            StrokeDirectMax.Clear();//
        }

        private void buttonTestForm_Click(object sender, RoutedEventArgs e)
        {
            int rightForm;
            rightForm  =0;
            if (PureWord == 0)
            {
                for (int i = 0; i < MyIP.Strokes.Count; i++)
                {
                    Stroke st = MyIP.Strokes[i];
                    if ((int)MyStrokeArrayWrite[i] == 0)
                    {
                        if (st.DrawingAttributes.Color == Colors.White)
                        {
                            rightForm++;
                        }
                        else
                        {
                            st.DrawingAttributes.Color = Colors.HotPink;

                        }

                    }
                    else
                    {
                        if (st.DrawingAttributes.Color == Colors.Yellow)
                        {
                            rightForm++;
                        }
                        else
                        {
                            st.DrawingAttributes.Color = Colors.Green;

                        }
                    }
                }
                double rightFormPer;
                rightFormPer = ((double)rightForm) / (MyIP.Strokes.Count);
                textBoxRightForm.Text = Convert.ToString(rightFormPer);
            }
            
        }

        private void buttonModifyForm_Click(object sender, RoutedEventArgs e)//这个地方有待于进一步修改
        {
            for (int i = 0; i < MyIP.Strokes.Count; i++)
            {
                Stroke st = MyIP.Strokes[i];
                if (st.DrawingAttributes.Color == Colors.HotPink)//将文字误认为表格
                {
                    st.DrawingAttributes.Color = Colors.White;
 
                }
                else if (st.DrawingAttributes.Color == Colors.Green)//将表格误认为文字
                {
                    st.DrawingAttributes.Color = Colors.Yellow;

                }
            }

        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            buttonDivAreaStren_Click(sender,e);
            //System.Threading.Thread.Sleep(2000);

            buttonFormAreaLoc_Click(sender, e);


            
        }





       

        

  




    }
}
