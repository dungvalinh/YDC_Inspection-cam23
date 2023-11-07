using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cognex.VisionPro;
using Basler.Pylon;
using Cognex.VisionPro.ToolBlock;
using ActUtlTypeLib;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Threading;
using System.Windows.Documents;
using System.Collections.Generic;

namespace YDC_Inspection
{
    public partial class Form1 : Form
    {
        
        Camera camera = new Camera("24687446");
        private CogToolBlock cogtoolblock = null;
        private CogToolBlock cogtoolblock1 = null;
        private ActUtlType plc = new ActUtlType();
        PixelDataConverter pxConvert = new PixelDataConverter();
        Camera camera1 = new Camera("24687482");
       public int run = 0; public int run1 = 0; public int run2 = 0; public int run3 = 0;
        Bitmap GrabResult2Bmp1(IGrabResult grabResult)
        {
            Bitmap b = new Bitmap(grabResult.Width, grabResult.Height, PixelFormat.Format32bppRgb);
            BitmapData bmpData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, b.PixelFormat);
            pxConvert.OutputPixelFormat = PixelType.BGRA8packed;
            IntPtr bmpIntpr = bmpData.Scan0;
            pxConvert.Convert(bmpIntpr, bmpData.Stride * b.Height, grabResult);
            b.UnlockBits(bmpData);
            return b;
        }
        public Form1()
        {
            
            InitializeComponent();
            cogtoolblock =   (CogToolBlock)CogSerializer.LoadObjectFromFile(@"C:\Users\YDC\Desktop\job_file\Cam_Right.vpp")  ;
            cogtoolblock.Changed += new CogChangedEventHandler(cogtoolblock_Changed);
            //cogtoolblock.Running += Cogtoolblock_Running;
            cogtoolblock.Inputs["Input"].Value = null ;
            camera.CameraOpened += Configuration.AcquireContinuous;
            camera.Open();
            camera.Parameters[PLCameraInstance.MaxNumBuffer].SetValue(5);
            cogtoolblock1 = CogSerializer.LoadObjectFromFile(@"C:\Users\YDC\Desktop\job_file\Cam_Left.vpp") as CogToolBlock;
            // cogtoolblock1.Running += Cogtoolblock1_Running;
            cogtoolblock1.Changed += new CogChangedEventHandler(cogtoolblock1_Changed);
            cogtoolblock1.Inputs["Input"].Value = null;
            
                camera1.CameraOpened += Configuration.AcquireContinuous;
                camera1.Open();
                camera1.Parameters[PLCameraInstance.MaxNumBuffer].SetValue(3);
            
        }

        private void cogtoolblock1_Changed(object sender, CogChangedEventArgs e)
        {
            ICogRecord cogRecord = cogtoolblock1.CreateLastRunRecord();
            if (cogRecord != null)
            {
                cogRecordDisplay1.Record = cogRecord.SubRecords["CogPMAlignTool1.InputImage"];
            }
        }

        private void cogtoolblock_Changed(object sender, CogChangedEventArgs e)
        {
            ICogRecord cogRecord = cogtoolblock.CreateLastRunRecord();
            if(cogRecord!= null)
            {
                cogRecordDisplay2.Record = cogRecord.SubRecords["CogPMAlignTool1.InputImage"];
            }
        }

        private void Cogtoolblock1_Running(object sender, EventArgs e)
        {
            //CogRectangleAffine grp = new CogRectangleAffine(); grp.CenterX = 0; grp.CenterY = 50;
            //cogRecordDisplay1.Image = cogtoolblock1.Outputs["Image_Output"].Value as ICogImage;
            //cogRecordDisplay1.Record = lastRunRecord.SubRecords["InputImage"];
            //cogRecordDisplay1.Fit(true);
        }
      
        
        Bitmap GrabResult2Bmp(IGrabResult grabResult)
        {
            Bitmap b = new Bitmap(grabResult.Width, grabResult.Height, PixelFormat.Format32bppRgb);
            BitmapData bmpData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, b.PixelFormat);
            pxConvert.OutputPixelFormat = PixelType.BGRA8packed;
            IntPtr bmpIntpr = bmpData.Scan0;
            pxConvert.Convert(bmpIntpr, bmpData.Stride * b.Height, grabResult);
            b.UnlockBits(bmpData);
            return b;
        }
        
        private void Cogtoolblock_Running(object sender, EventArgs e)
        {
            //cogRecordDisplay2.Image = cogtoolblock.Outputs["Image_Output"].Value as ICogImage;
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            if (button2.BackColor == Color.LightGray)
            {
                
                plc.ActLogicalStationNumber = 1;
                plc.Open();
                plc.SetDevice("M603", 1);
                timer1.Enabled = true;
                timer1.Interval = 200;
                timer2.Enabled = true;
                timer2.Interval = 1000;
                button2.BackColor = Color.Lime;
                lbConnect.Text = "Connected";
                string type; int code;
                plc.GetCpuType(out type, out code);
                label4.Text = type;
            }
            else if (button2.BackColor == Color.Lime)
            {
                plc.SetDevice("M603", 0);
                plc.Close();
                timer1.Enabled = false;
                button2.BackColor = Color.LightGray;
                lbConnect.Text = "disconnected";
                label4.Text = "unknown";
                timer2.Enabled = false;
            }
        }
       
        private PixelDataConverter converter = new PixelDataConverter();
        private void btnLive_Click(object sender, EventArgs e)
        {

            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FrmJob frm = new FrmJob(cogtoolblock);
            frm.Show();
        }
        double ReAll = 0; double ReOk = 0; double ReNG = 0;double ReVe = 0;
        
        private void timer1_Tick(object sender, EventArgs e)
        {
           
            int value = 0;
            plc.GetDevice("D310", out value);
            if (value ==1)
            {
                               camera.StreamGrabber.Start();
                IGrabResult grabResult = camera.StreamGrabber.RetrieveResult(5000, TimeoutHandling.ThrowException);
                camera1.StreamGrabber.Start();
                IGrabResult grabResult1 = camera1.StreamGrabber.RetrieveResult(5000, TimeoutHandling.ThrowException);
                if (grabResult.GrabSucceeded && grabResult1.GrabSucceeded)
                {
                    ReAll++;
                    lbTrigg.BackColor = Color.Lime;
                    var image = new CogImage8Grey(GrabResult2Bmp(grabResult));
                    var image1 = new CogImage8Grey(GrabResult2Bmp(grabResult1));
                    var image_ = GrabResult2Bmp1(grabResult);
                    var image1_ = GrabResult2Bmp1(grabResult1);
                    cogtoolblock.Inputs["Input"].Value = image;
                    cogtoolblock.Run();
                    cogtoolblock1.Inputs["Input"].Value = image1;
                    cogtoolblock1.Run();
                    var result = cogtoolblock.Outputs["Result_1"].Value;
                    var result1 = cogtoolblock1.Outputs["Result_1"].Value;
                    var result_ = cogtoolblock.Outputs["Result_Fix"].Value;
                    var result_1 = cogtoolblock1.Outputs["Result_Fix"].Value;
                    if (result.ToString() == result1.ToString() && (int)result_ == 1 && (int)result_1 == 1)
                    {

                        switch (result)
                        {
                            case CogToolResultConstants.Accept:
                                ReOk++;
                                lbResult.Text = "OK";
                                lbResult.BackColor = Color.Lime;
                                plc.SetDevice("D312", 1);

                                break;
                            case CogToolResultConstants.Reject:
                            case CogToolResultConstants.Error:
                                //DialogResult rs = MessageBox.Show("Do You Want Verify?", "Note", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                //if (rs == DialogResult.Yes)
                                //{
                                //    ReVe++;
                                //    lbResult.Text = "Verify";
                                //    lbResult.BackColor = Color.DarkViolet;
                                //    plc.SetDevice("D312", 1);

                                //}
                                //else if (rs == DialogResult.No)
                                //{
                                ReNG++;
                                lbResult.Text = "NG";
                                lbResult.BackColor = Color.Red;
                                plc.SetDevice("D313", 1);

                                //}
                                break;
                            default:
                                label1.Text = "";
                                break;
                        }
                    }
                    else if (result.ToString() != result1.ToString() || (int)result_ == 0 || (int)result_1 == 0)
                    {
                        //DialogResult rs = MessageBox.Show("Do You Want Verify?", "Note", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        //if (rs == DialogResult.OK)
                        //{
                        //    ReVe++;
                        //    lbResult.Text = "Verify";
                        //    lbResult.BackColor = Color.DarkViolet;
                        //    plc.SetDevice("D312", 1);

                        //}
                        //else if (rs == DialogResult.No)
                        //{

                        ReNG++;
                        lbResult.Text = "NG";
                        lbResult.BackColor = Color.Red;
                        plc.SetDevice("D313", 1);

                        // }

                    }
                    if (result.ToString() == "Accept" && (int)result_ == 1)
                    {
                        string filename = System.String.Format(@"F:\ImageLog\Camera_Right\OK_Image\{0}.bmp", lbCode.Text);
                        image_.Save(filename, ImageFormat.Bmp);
                        
                    }
                    else if (result.ToString() == "Reject" || (int)result_ == 0)
                    {
                        string filename1 = System.String.Format(@"F:\ImageLog\Camera_Right\NG_Image\{0}.bmp", lbCode.Text);
                        image_.Save(filename1, ImageFormat.Bmp);
                        
                    }

                    if (result1.ToString() == "Accept" && (int)result_1 == 1)
                    {
                        string filename2 = System.String.Format(@"F:\ImageLog\Camera_Left\OK_Image\{0}.bmp", lbCode.Text);
                        image1_.Save(filename2, ImageFormat.Bmp);
                  
                    }
                    else if (result1.ToString() == "Reject" || (int)result_1 == 0)
                    {
                        string filename3 = System.String.Format(@"F:\ImageLog\Camera_Left\NG_Image\{0}.bmp", lbCode.Text);
                        image1_.Save(filename3, ImageFormat.Bmp);
                      
                    }
                }
                camera.StreamGrabber.Stop();
                camera1.StreamGrabber.Stop();
            }
            else if(value ==0)
            {
                lbTrigg.BackColor = Color.Silver;
            }

            double ve1 = (ReVe / ReAll) * 100;
            double ok1 = (ReOk / ReAll) * 100;
            double ng1 = (ReNG / ReAll) * 100;
            string ok = Math.Round(ok1, 2).ToString();
            string ng = Math.Round(ng1, 2).ToString();
            string ve = Math.Round(ve1, 2).ToString();
            numAll.Text = ReAll.ToString();
            numOk.Text = ReOk.ToString() + "(" + ok + "%" + ")";
            numNg.Text = ReNG.ToString() + "(" + ng + "%" + ")";
            lbVerify.Text = ReVe.ToString() + "(" + ve + "%" + ")";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
           // CogSerializer.SaveObjectToFile(cogtoolblock, jobFile);
            DialogResult dgResult = MessageBox.Show("Do you want to exit?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            plc.SetDevice("M603", 0);
            if (dgResult == DialogResult.OK)
            {
                plc.SetDevice("M603", 0);
                Application.Exit();
            }
            else if(dgResult == DialogResult.No)
            {
                e.Cancel = true;
            }
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            camera.StreamGrabber.Start();
            IGrabResult grabResult = camera.StreamGrabber.RetrieveResult(5000, TimeoutHandling.ThrowException);
            camera1.StreamGrabber.Start();
            IGrabResult grabResult1 = camera1.StreamGrabber.RetrieveResult(5000, TimeoutHandling.ThrowException);
            if (grabResult.GrabSucceeded && grabResult1.GrabSucceeded)
            {
                ReAll++;
                lbTrigg.BackColor = Color.Lime;
                var image = new CogImage8Grey(GrabResult2Bmp(grabResult));
                var image1 = new CogImage8Grey(GrabResult2Bmp(grabResult1));
                var image_ = GrabResult2Bmp1(grabResult) ;
                var image1_ = GrabResult2Bmp1(grabResult1) ;
                cogtoolblock.Inputs["Input"].Value = image;
                cogtoolblock.Run();
                cogtoolblock1.Inputs["Input"].Value = image1;
                cogtoolblock1.Run();
                
                
                var result = cogtoolblock.Outputs["Result_1"].Value;
                var result1 = cogtoolblock1.Outputs["Result_1"].Value;
                var result_ = cogtoolblock.Outputs["Result_Fix"].Value;
                var result_1 = cogtoolblock1.Outputs["Result_Fix"].Value;
               
                if (result.ToString() == result1.ToString() && (int)result_==1 && (int)result_1==1)
                {

                    switch (result)
                    {
                        case CogToolResultConstants.Accept:
                            ReOk++;
                            lbResult.Text = "OK";
                            lbResult.BackColor = Color.Lime;
                            plc.SetDevice("D312", 1);
                           
                            break;
                        case CogToolResultConstants.Reject:
                        case CogToolResultConstants.Error:
                            //DialogResult rs = MessageBox.Show("Do You Want Verify?", "Note", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            //if (rs == DialogResult.Yes)
                            //{
                            //    ReVe++;
                            //    lbResult.Text = "Verify";
                            //    lbResult.BackColor = Color.DarkViolet;
                            //    plc.SetDevice("D312", 1);
                           
                            //}
                            //else if (rs == DialogResult.No)
                            //{
                            ReNG++;
                                lbResult.Text = "NG";
                                lbResult.BackColor = Color.Red;
                                plc.SetDevice("D313", 1);
                               
                            //}
                            break;
                        default:
                            label1.Text = "";
                            break;
                    }
                }
                else if (result.ToString() != result1.ToString()|| (int)result_ == 0 || (int)result_1 == 0)
                {
                    //DialogResult rs = MessageBox.Show("Do You Want Verify?", "Note", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    //if (rs == DialogResult.OK)
                    //{
                    //    ReVe++;
                    //    lbResult.Text = "Verify";
                    //    lbResult.BackColor = Color.DarkViolet;
                    //    plc.SetDevice("D312", 1);

                    //}
                    //else if (rs == DialogResult.No)
                    //{
                   
                    ReNG++;
                        lbResult.Text = "NG";
                        lbResult.BackColor = Color.Red;
                        plc.SetDevice("D313", 1);

                   // }

                }
                if (result.ToString() == "Accept" && (int)result_ == 1)
                {
                    string filename = System.String.Format(@"F:\ImageLog\Camera_Right\OK_Image\{0}.bmp", lbCode.Text);
                    image_.Save(filename, ImageFormat.Bmp);
                    run++;
                }
                else if (result.ToString() == "Reject" || (int)result_ == 0)
                {
                    string filename1 = System.String.Format(@"F:\ImageLog\Camera_Right\NG_Image\{0}.bmp", lbCode.Text);
                    image_.Save(filename1, ImageFormat.Bmp);
                    run1++;
                }

                if(result1.ToString() == "Accept" && (int)result_1 == 1)
                {
                    string filename2 = System.String.Format(@"F:\ImageLog\Camera_Left\OK_Image\{0}.bmp", lbCode.Text);
                    image1_.Save(filename2, ImageFormat.Bmp);
                    run2++;
                }
                else if (result1.ToString() == "Reject" || (int)result_1 == 0)
                {
                    string filename3 = System.String.Format(@"F:\ImageLog\Camera_Left\NG_Image\{0}.bmp", lbCode.Text);
                    image1_.Save(filename3, ImageFormat.Bmp);
                    run3++;
                }




            }
            
            camera.StreamGrabber.Stop();
            camera1.StreamGrabber.Stop();
            double ve1 = (ReVe / ReAll) * 100;
            double ok1 = (ReOk / ReAll) * 100;
            double ng1 = (ReNG / ReAll) * 100;
            string ok = Math.Round(ok1, 2).ToString();
            string ng = Math.Round(ng1, 2).ToString();
            string ve = Math.Round(ve1, 2).ToString();
            numAll.Text = ReAll.ToString();
            numOk.Text = ReOk.ToString() + "(" + ok + "%" + ")";
            numNg.Text = ReNG.ToString() + "(" + ng + "%" + ")";
            lbVerify.Text = ReVe.ToString() + "(" + ve + "%" + ")";
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void splitContainer7_DoubleClick(object sender, EventArgs e)
        {
           
               
        }

        private void splitContainer8_DoubleClick(object sender, EventArgs e)
        {
            

        }

        private void cogRecordDisplay1_DoubleClick(object sender, EventArgs e)
        {
            FrmJob frm = new FrmJob(cogtoolblock1);
            frm.Show();
        }

        private void cogRecordDisplay2_DoubleClick(object sender, EventArgs e)
        {
            FrmJob1 frm = new FrmJob1(cogtoolblock);
            frm.Show();
        }
        
        private void button4_Click(object sender, EventArgs e)
        {
            
           
           
            ReAll = 0; ReOk = 0;
            ReNG = 0;
            numAll.Text = " ";
            numOk.Text = " ";
            numNg.Text = " ";
            lbResult.Text = " ";
            lbResult.BackColor = Color.DarkGray;
        }

        private void cogRecordDisplay1_Enter(object sender, EventArgs e)
        {

        }

        private void numAll_Click(object sender, EventArgs e)
        {

        }

        private void cogRecordDisplay2_Enter(object sender, EventArgs e)
        {

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            string c = " ";
            int data = 5063;
            for (int i = 0; i < 8; i++)
            {
                data += 1;

                int value = 0;
                plc.GetDevice("D"+data.ToString(), out value);
                char c1 = (char)(value & 0xff);
                char c2 = (char)(value >> 8);
                c += c1.ToString() + c2.ToString();

            }
            lbCode.Text = c;
        }
    }
}
