using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace TTS_InfoMaker2
{
    class DataInfo : IComparable<DataInfo>
    {
        int index;
        string script;

        public int Index
        {
            get
            {
                return index;
            }
        }
        public string Script
        {
            get
            {
                return script;
            }
        }
        public DataInfo(int _index , string _script)
        {
            index = _index;
            script = _script;
        }

        public void PrintValue()
        {
            Console.WriteLine("{0} :: {1} ", index, script); // {2}", fi.Name,
        }

        public int CompareTo(DataInfo other)
        {
            if(other == null)
            {
                return 1;
            }
            else
            {
                return this.index.CompareTo(other.index);
            }
            
        }
    }

    public partial class MainWindow : Window
    {
        const string dataInfoFolderName = "DataInfo";
        const string audioFolderName = "Audios";

        string path;
        FileInfo[] files;
        List<DataInfo> dataInfos = new List<DataInfo>();

        public MainWindow()
        {
            InitializeComponent();
        }

        // 폴더 경로 설정
        private void FileOpenBtn_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog(); // 폴더 브라우저 다이얼로그 개체 생성
            folderDlg.ShowDialog(); // 창 띄우기 
            path = folderDlg.SelectedPath; // 선택한 경로를 가져온다.
            Console.WriteLine(path);
        }

        /// <summary>
        /// 제이슨 파일 생성 및 한글 제거
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            // 폴더에 있는 파일들 가져오기
            var file = new FileInfo(path);
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            files = directoryInfo.GetFiles();

            for (int i = 0; i < files.Length; i++)
            {
                #region >> DataEx << 
                // audio_0__안녕하세요_
                // audio_1_저는_당신의_3D_기반_실감_메이크업_아티스트_체험을~
                // audio_27_그럼__어디_한번_3D_스캐너를_사용해서_고객의_얼굴~
                // 앞 8글자를 기준으로 자른다.        
                #endregion
                string indexTemp = files[i].Name.Substring(0, 8);                                        // 앞 8글자 자르기 
                string scriptTemp = files[i].Name.Substring(8, files[i].Name.Length - indexTemp.Length); // 8글자 뒤는 모두 스크립트
                int indexinfo = int.Parse(Regex.Replace(indexTemp, @"\D", ""));                          // 숫자만 추출

                dataInfos.Add(new DataInfo(indexinfo, scriptTemp));
                Console.WriteLine("@@@ files ::  {0}: {1}: ", indexinfo, scriptTemp); // {2}", fi.Name,
            }

            dataInfos.Sort(delegate (DataInfo x, DataInfo y) // 소팅
            {
                return x.CompareTo(y);
            });

            var dataInfoJsonFile = new JArray();
            foreach (DataInfo item in dataInfos)
            {
                item.PrintValue();
                JObject jObject = new JObject();
                jObject.Add("index",item.Index); 
                jObject.Add("script",item.Script); 
                dataInfoJsonFile.Add(jObject);

            }

            string floderPath = System.IO.Path.Combine(path, dataInfoFolderName);

            try
            {

                System.IO.DirectoryInfo selectFolderDirectory = new DirectoryInfo(floderPath); // 해당 디텍 정보 설정
                if (selectFolderDirectory.Exists) // 디텍이 존재하는가
                {
                    // 존재 하면 그 디텍을 사용
                }
                else // 존재하지 않으면 
                {
                    selectFolderDirectory.Create(); // 디텍 새로 생성
                }

                Console.WriteLine("selectFolderDirectory.FullName:: " + selectFolderDirectory.FullName);

                string textFileName = "Audio_"+TexBox_FileName.Text.Trim() + ".json"; // 텍스트 파일 이름 설정
                string textFilePath = System.IO.Path.Combine(selectFolderDirectory.FullName, textFileName);  // 텍스트 파일 경로 설정
                Console.WriteLine("textFilePath :: "+textFilePath);
                Console.WriteLine("@@@@@@@@asdsadsafdsafasdfdas ::: " + dataInfoJsonFile.ToString());
                
                System.IO.File.WriteAllText(textFilePath, dataInfoJsonFile.ToString()); // 알아서 없으면 파일 생성한다.

                RenameToIndex(files, "wav");// 텍스트 파일 수정
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message.ToString());
            }

            PopUP popUp = new PopUP();

            popUp.Owner = this;
            popUp.Top = this.Top + 140;
            popUp.Left = this.Left + 100;
            //popUp.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dataInfos.Clear();
            popUp.ShowDialog();
        }

        /// <summary>
        /// 현재 파일 이름들을 Index 값으로 변경한다. 
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        private void RenameToIndex(FileInfo[] files, string extension)
        {
            // 파일들의 이름과 경로를 합치고
            // 새로운 경로를 통해 이름을 바꾼다.
            foreach (FileInfo item in files)
            {
                //이전 경로
                string filePath = item.DirectoryName;
                string oldFileName = item.Name;

                // 이름 가져와서 
                string indexFileName = item.Name.Substring(0, 8);
                int indexinfo = int.Parse(Regex.Replace(indexFileName, @"\D", ""));
                //Index로 변경

                // 새로운 디텍토리 체크
                DirectoryInfo selectFolderDirectory = new DirectoryInfo(System.IO.Path.Combine(path,audioFolderName)); // 해당 디텍 정보 설정

                if (selectFolderDirectory.Exists  == false) // 디텍이 존재하는가
                {
                    // 존재하지 않으면 
                    selectFolderDirectory.Create(); // 디텍 새로 생성
                }
                else 
                {
                    // 존재 하면 그 디텍을 사용
                }
                // 이름 재설정
                string oldFile = filePath + "\\" + oldFileName;
                string newFIle = filePath + "/" + audioFolderName + "\\" +"TTS_"+ indexinfo + "." + extension;
                Console.WriteLine("oldFile : " + oldFile + " # newFIle : " + newFIle);
                System.IO.File.Move(oldFile, newFIle);
            }
        }

        private void FileName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
