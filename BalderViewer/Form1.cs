using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Eklekto.Helpers;
using FibroscanProcessor;
using FibroscanProcessor.Elasto;

namespace BalderViewer
{
    public partial class Form1 : Form
    {
        Image sourceImage;
        FibroscanImage WorkingImage;

        private List<PictureBox> pictures;
        private List<Label> imageLabels;
        private List<ComboBox> teachStatusComboBox;
        private List<Label> simpleStatusLabel;

        private LearningElastoClassificator LearningClassificator;
        private CommonTrainInfo AgeInfo;


        public Form1()
        {
            InitializeComponent();

            pictures = new List<PictureBox>
            {
                sourcePicture, elastoPicture, kuwaharaPicture, binarizationPicture, edgePicture, morphologyPicture,cropPicture,
                choosingPicture, approximationPicture, sourceModMPicture, outModMPicture, sourceModAPicture, outModAPicture, productionPicture
            };
            imageLabels = new List<Label>
            {
                label22, label23, label24, label25, label26, label27,
                label28, label29, label30, label31, label32
            };
            teachStatusComboBox = new List<ComboBox>
            {
                elastoStatusBox, modMBox, modABox
            };
            simpleStatusLabel = new List<Label>
            {
                simpleElastoStatus, simpleModMStatus, simpleModAStatus
            };

        }

        private void LoadImage()
        {
            openSourceFileDialog.Filter = "image (JPEG) files (*.jpg)|*.jpg|All files (*.*)|*.*";
            AllClear();
            imageLabels.ForEach(label => label.Visible = false);
            teachStatusComboBox.ForEach(box => box.Visible = false);
            simpleStatusLabel.ForEach(label => label.Visible = false);
            teachButton.Visible = false;
            buttonNextImage.Enabled = false;
            buttonPrevImage.Enabled = false;

            if (openSourceFileDialog.ShowDialog() == DialogResult.OK)
            {
                sourceImage = Image.FromFile(openSourceFileDialog.FileName);
                sourcePicture.Image = sourceImage;
                imagePath.Text = openSourceFileDialog.FileName;
                buttonNextImage.Enabled = true;
                buttonPrevImage.Enabled = true;
            }
        }

        private void StartImageVerification()
        {
            if (sourcePicture.Image == null)
                LoadImage();
            if (sourcePicture.Image != null)
            {
                BoxClear();
                imageLabels.ForEach(label => label.Visible = true);
                teachStatusComboBox.ForEach(box => box.Visible = true);
                teachStatusComboBox.ForEach(box => box.SelectedItem = "Uncertain");
                simpleStatusLabel.ForEach(label => label.Visible = true);
                teachButton.Visible = true;

                WorkingImage = new FibroscanImage(sourceImage, true);
                VerificationStatus elastoStatus = ElastogramVerification();
                Ultrasoundverification();

                //Production code
                if (productionCheckBox.Checked)
                {
                    FibroscanImage prod = new FibroscanImage(sourceImage);
                    productionPicture.Image = prod.Merged;
                }


                if (savingStepsCheckBox.Checked)
                    SaveSteps();
                if (saveClassificationCheckBox.Checked)
                    SaveClassification(elastoStatus);
            }
        }

        private VerificationStatus ElastogramVerification()
        {
            long timer = 0;

            elastoPicture.Image = WorkingImage.Step1LoadElastogram();

            WorkingImage.Step2ElastoWithoutLine();

            kuwaharaPicture.Image = WorkingImage.Step3KuwaharaElasto((int)numericUpDown1.Value);

            if (simpleBinRadioButton.Checked)
                binarizationPicture.Image = WorkingImage.Step4SimpleBinarization(ref timer, (byte)upDownBinarThreshold.Value);
            if (niblackRadioButton.Checked)
                binarizationPicture.Image = WorkingImage.Step4NiblackBinarization(ref timer, (double)upDownBinarizationK.Value, (int)upDownBinarLocalRadius.Value);
            if (sauvolaRadioButton.Checked)
                binarizationPicture.Image = WorkingImage.Step4SauvolaBinarization(ref timer, (double)upDownBinarizationK.Value, (int)upDownBinarLocalRadius.Value);
            if (wolfRadioButton.Checked)
                binarizationPicture.Image = WorkingImage.Step4WolfJoulionBinarization(ref timer, (double)upDownBinarizationK.Value, (int)upDownBinarLocalRadius.Value);
            if (morphNiblackRadioButton.Checked)
                binarizationPicture.Image = WorkingImage.Step4MorphologyNiblackBinarization(ref timer, (double)upDownBinarizationK.Value, (int)upDownBinarLocalRadius.Value,
                                                                (int)upDownBinarGlobalRadius.Value, (byte)upDownBinarThreshold.Value);
            if (morphOtsuRadioButton.Checked)
                binarizationPicture.Image = WorkingImage.Step4SimpleMorphologyBinarization(ref timer, (int)upDownBinarGlobalRadius.Value, (byte)upDownBinarThreshold.Value);
            resultBox.Items.Add("Binarization time:       " + timer);

            edgePicture.Image = WorkingImage.Step5EdgeRemoving(ref timer, (int)upDownEdgeLeft1.Value, (int)upDownEdgeLeft1Central.Value, (int)upDownEdgeLeft2.Value,
                (int)upDownEdgeLeft2Central.Value, (int)upDownEdgeRight.Value, (int)upDownEdgeRightCentral.Value);

            morphologyPicture.Image = WorkingImage.Step6Morphology((int)numericUpDown3.Value);

            cropPicture.Image = WorkingImage.Step7CropObjects((int)upDownCropStep.Value, (int)upDownCropDistance.Value);

            choosingPicture.Image = WorkingImage.Step8ChooseOneObject(ref timer, 0.55, 0.65);

            approximationPicture.Image = WorkingImage.Step9Approximation(ref timer, (double)upDownRansacSample.Value, (double)upDownRansacOutliers.Value,
                (int)upDownRansacIterations.Value);

            resultBox.Items.Add("RANSAC time:        " + timer);

            signatureBox.Items.Add("Fibroline Angle:      " + Math.Round(WorkingImage.Fibroline.Equation.Angle, 2));
            signatureBox.Items.Add("Left Angle:              " + Math.Round(WorkingImage.WorkingSignature.LeftAngle, 2));
            signatureBox.Items.Add("Right Angle:            " + Math.Round(WorkingImage.WorkingSignature.RightAngle, 2));
            signatureBox.Items.Add("Left RSquare:         " + Math.Round(WorkingImage.WorkingSignature.RSquareLeft, 2));
            signatureBox.Items.Add("Right RSquare:        " + Math.Round(WorkingImage.WorkingSignature.RSquareRight, 2));
            signatureBox.Items.Add("Left Relative Est:      " + Math.Round(WorkingImage.WorkingSignature.RelativeEstimationLeft, 2));
            signatureBox.Items.Add("Right Relative Est:     " + Math.Round(WorkingImage.WorkingSignature.RelativeEstimationRight, 2));
            signatureBox.Items.Add("Blob Area:                " + WorkingImage.WorkingSignature.Area);

            VerificationStatus elastoStatus;
            if (isLearningClassificatorCheckBox.Checked)
                elastoStatus = smartElastogramVerification();
            else
                elastoStatus = WorkingImage.Step10Classify();

            resultBox.Items.Add("Elastogram is " + elastoStatus);
            simpleElastoStatus.Text = elastoStatus.ToString();
            return elastoStatus;
        }

        private void Ultrasoundverification()
        {
            sourceModMPicture.Image = WorkingImage.Step11LoadUltrasoundM((double)upDownBrightPixelLimit.Value, (int)upDownUsDeviationStreak.Value);
            VerificationStatus umms = VerificationStatus.NotCalculated;

            outModMPicture.Image = WorkingImage.Step15DrawBrightLines(ref umms, (int)upDownLimitUsBrightness.Value, (int)upDownBrightPixelLimit.Value, (int)upDownBrightLinesLimit.Value);

            resultBox.Items.Add("UltraSoundModM is " + umms);
            simpleModMStatus.Text = umms.ToString();

            signatureBox.Items.Add("Mod M bright lines:  " +
                                   WorkingImage.WorkingUltrasoundModM.getBrightLines((int)upDownLimitUsBrightness.Value, (int)upDownBrightPixelLimit.Value).Count);

            sourceModAPicture.Image = WorkingImage.Step13LoadUltrasoundA();
            VerificationStatus umas = VerificationStatus.NotCalculated;

            outModAPicture.Image = WorkingImage.Step14DrawUltraSoundApproximation(ref umas, (int)upDownRelativeEstimationLimit.Value);

            signatureBox.Items.Add("ModA Estimation:   " + Math.Round(WorkingImage.WorkingUltrasoundModA.RelativeEstimation, 2));
            signatureBox.Items.Add("ModA RSquare        : " + Math.Round(WorkingImage.WorkingUltrasoundModA.RSquare));
            resultBox.Items.Add("UltraSoundModA is " + umas);
            simpleModAStatus.Text = umms.ToString();


        }

        private void FolderVerification()
        {
            var firstFiles = Directory.EnumerateFiles(Path.GetDirectoryName(imagePath.Text)).ToList();
            int filesNumber = Math.Min(firstFiles.Count(), (int)upDownFilesNumber.Value);
            for (int i = 0; i < filesNumber; i++)
            {
                BoxClear();
                if (Path.GetExtension(firstFiles[i]) == ".jpg")
                {
                    imagePath.Text = firstFiles[i];
                    sourceImage = Image.FromFile(imagePath.Text);
                    sourcePicture.Image = sourceImage;
                    StartImageVerification();
                }
                commonStatBox.Items.Add(i + 1 + " files processed");
            }
        }

        private void TrainClassificator()
        {
            List<ElastogramSignatura> trainingSignatura = readSignatureFromFile(trainFileTextBox.Text);
            List<ElastogramSignatura> precedentSignatura = readSignatureFromFile(precedentsFileBox.Text);
            List<double> classificationRadiueses = new List<double>
            {
                (double)upDownSignatureArea.Value,
                (double)upDownSignatureAngleFibro.Value,
                (double)upDownSignatureAngleLeft.Value,
                (double)upDownSignatureAngleRight.Value,
                (double)upDownSignatureR2Left.Value,
                (double)upDownSignatureR2Right.Value,
                (double)upDownSignatureRELeft.Value,
                (double)upDownSignatureRERight.Value,
            };
             LearningClassificator = new LearningElastoClassificator(trainingSignatura, precedentSignatura, classificationRadiueses);
             AgeInfo = LearningClassificator.Train((int)upDownAgeSize.Value, (int)upDownAgeNum.Value,
                (double)upDownTeachConvergence.Value, (double)upDownTeachError.Value);
            List<ElastogramSignatura> outPrecedents = LearningClassificator.Precedents;
            precedentBox.Items.Clear();
            outPrecedents.ForEach(signature => precedentBox.Items.Add(string.Join(" ", signature.NormalizedSignatura)));
            AgeInfo.Info.ForEach(age => trainInfoBox.Items.Add(string.Join(" ", age.infoList)));
            trainInfoBox.Items.Add(outPrecedents.Count);
        }

        private VerificationStatus smartElastogramVerification()
        {
            if (AgeInfo.IsTrained)
                return LearningClassificator.Classiffy(WorkingImage.WorkingSignature);

            return VerificationStatus.NotCalculated;
        } 



        #region Saving
        private void SaveOneImageToFolder()
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                for (int i = 0; i < pictures.Count; i++)
                    if (pictures[i].Image != null)
                        pictures[i].Image.Save(folderBrowserDialog.SelectedPath + "//" + i + ".jpg");
        }

        private void SaveSteps()
        {
            String savingFolder = Path.GetDirectoryName(imagePath.Text) + "//ResultSteps//" + Path.GetFileNameWithoutExtension(imagePath.Text);
            Directory.CreateDirectory(savingFolder);
            for (int i = 0; i < pictures.Count; i++)
                if (pictures[i].Image != null)
                    pictures[i].Image.Save(savingFolder + "//" + i + ".jpg");
        }

        private void SaveClassification(VerificationStatus status)
        {
            String savingFolder = Path.GetDirectoryName(imagePath.Text) + "//Classification//" + status;
            Directory.CreateDirectory(savingFolder);
            pictures[0].Image.Save(savingFolder + "//" + Path.GetFileName(imagePath.Text));
        }

        private void saveRightAnswerToCsv()
        {
            try
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                foreach (string line in File.ReadLines(trainFileTextBox.Text))
                {
                    dict.Add(line.Split(' ')[0], line.Substring(line.IndexOf(' ') + 1));
                }
                ConcurrentDictionary<string, string> csvElementList = new ConcurrentDictionary<string, string>(dict);
                if (!String.IsNullOrWhiteSpace(elastoStatusBox.Text) && !String.IsNullOrWhiteSpace(imagePath.Text))
                {
                    string output = string.Join(" ", WorkingImage.WorkingSignature.NormalizedSignatura);
                    csvElementList.AddOrUpdate(imagePath.Text, output + " " + elastoStatusBox.Text);
                    commonStatBox.Items.Add("Learning List count is " + csvElementList.Count);
                }
                File.WriteAllText(trainFileTextBox.Text,
                    string.Join(Environment.NewLine, csvElementList.Select(d => d.Key + " " + d.Value).OrderBy(key => key)));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void saveSignatura()
        {
            using (StreamWriter file =
                             new StreamWriter(trainFileTextBox.Text, true))
            {
                string output = string.Join(" ", WorkingImage.WorkingSignature.NormalizedSignatura);
                file.WriteLine(imagePath.Text + " " + output + " " + elastoStatusBox.Text);
            }
        }

        #endregion

        #region buttons
        private void buttonNextImage_Click(object sender, EventArgs e)
        {
            try
            {
                AllClear();
                imagePath.Text = Directory.EnumerateFiles(Path.GetDirectoryName(imagePath.Text))
                    .Skip(GetCurrentFileImdex(imagePath.Text) + 1)
                    .First();
                sourceImage = Image.FromFile(imagePath.Text);
                sourcePicture.Image = sourceImage;
                StartImageVerification();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void buttonPrevImage_Click(object sender, EventArgs e)
        {
            try
            {
                AllClear();
                imagePath.Text = Directory.EnumerateFiles(Path.GetDirectoryName(imagePath.Text))
                    .Skip(GetCurrentFileImdex(imagePath.Text) - 1)
                    .First();
                sourceImage = Image.FromFile(imagePath.Text);
                sourcePicture.Image = sourceImage;
                StartImageVerification();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void groupProcessingButton_Click(object sender, EventArgs e)
        {
            if (sourcePicture.Image == null)
                LoadImage();
            if (sourcePicture.Image != null)
                FolderVerification();
        }

        private void teachFileLoadButton_Click(object sender, EventArgs e)
        {
            if (openCsvFileDialog.ShowDialog() == DialogResult.OK)
            {
                trainFileTextBox.Text = openCsvFileDialog.FileName;
            }
        }

        private void precedentsLoadButton_Click(object sender, EventArgs e)
        {
            if (openCsvFileDialog.ShowDialog() == DialogResult.OK)
            {
                precedentsFileBox.Text = openCsvFileDialog.FileName;
            }
        }

        private void teachButton_Click(object sender, EventArgs e)
        {
            saveRightAnswerToCsv();
        }

        private void TrainButton_Click(object sender, EventArgs e)
        {
            TrainClassificator();
        }
        #endregion


        #region Helpers

        private List<ElastogramSignatura> readSignatureFromFile(string path)
        {
            List<ElastogramSignatura> elastoSignatura = new List<ElastogramSignatura>();
            foreach (string line in File.ReadLines(path))
            {
                if (line.Length==0)
                    break;
                elastoSignatura.Add(stringToSignatura(line.Substring(line.IndexOf(' ') + 1), ' '));
            }
            return elastoSignatura;
        } 

        private ElastogramSignatura stringToSignatura(string signaturaString, char separator)
        {
            string[] numbersString = signaturaString.Split(separator);
            if (numbersString.Length != ElastogramSignatura.Size + 1)
                throw new ArgumentOutOfRangeException();
            List<double> signatureInput = new List<double>();
            for (int i = 0; i < ElastogramSignatura.Size; i++)
                signatureInput.Add(Convert.ToDouble(numbersString[i]));
            VerificationStatus answer = (VerificationStatus) Enum.Parse(typeof(VerificationStatus), numbersString.Last());
            return new ElastogramSignatura(signatureInput, answer , true);
        }

        private int GetCurrentFileImdex(string text)
        {
            var filesEnumerator = Directory.EnumerateFiles(Path.GetDirectoryName(text));
            int fileIndex = 0;

            foreach (var file in filesEnumerator)
            {
                if (file == imagePath.Text)
                    break;
                fileIndex++;
            }
            return fileIndex;
        }
        private void AllClear()
        {
            pictures.ForEach(picture =>
            {
                picture.Image = null;
                picture.Invalidate();
            });
            BoxClear();
        }

        private void BoxClear()
        {
            signatureBox.Items.Clear();
            resultBox.Items.Clear();
            commonStatBox.Items.Clear();
        }
        #endregion

        #region Hotkeys

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Right))
            {
                try
                {
                    AllClear();
                    imagePath.Text = Directory.EnumerateFiles(Path.GetDirectoryName(imagePath.Text))
                            .Skip(GetCurrentFileImdex(imagePath.Text) + 1)
                            .First();
                    sourceImage = Image.FromFile(imagePath.Text);
                    sourcePicture.Image = sourceImage;
                    StartImageVerification();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                return true;
            }

            if (keyData == (Keys.Left))
            {
                try
                {
                    AllClear();
                    imagePath.Text = Directory.EnumerateFiles(Path.GetDirectoryName(imagePath.Text))
                            .Skip(GetCurrentFileImdex(imagePath.Text) - 1)
                            .First();
                    sourceImage = Image.FromFile(imagePath.Text);
                    sourcePicture.Image = sourceImage;
                    StartImageVerification();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                return true;
            }

            if (keyData == (Keys.Escape))
            {
                {
                    DialogResult rsl = MessageBox.Show("Ты не посмеешь уйти, когда я анализирую изображения!", "Одумайся!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (rsl == DialogResult.No)
                        Application.Exit();
                }
                return true;
            }
            if (keyData == (Keys.Space))
            {
                LoadImage();
                return true;
            }
            if (keyData == (Keys.Enter))
            {
                StartImageVerification();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion

        #region Casual methods

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult rsl = MessageBox.Show("Не смей уходить, когда я анализирую изображения!", "Одумайся!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (rsl == DialogResult.No)
                Application.Exit();
        }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            LoadImage();
        }
        private void loadToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            LoadImage();
        }
        private void loadButton_Click(object sender, EventArgs e)
        {
            LoadImage();
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            StartImageVerification();
        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            SaveOneImageToFolder();
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveOneImageToFolder();
        }
        private void processingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartImageVerification();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            StartImageVerification();
        }
        private void pictureBox_Click(object sender, EventArgs e)
        {
            BigImage fBigImage = new BigImage(((PictureBox)sender).Image);
            fBigImage.ShowDialog();
        }
        private void pictureBox_Saving(object sender, EventArgs e)
        {
            Image img = ((PictureBox)sender).Image;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
                if (img != null)
                    img.Save(saveFileDialog.FileName + ".jpg");
        }



        #endregion

       
    }
}
