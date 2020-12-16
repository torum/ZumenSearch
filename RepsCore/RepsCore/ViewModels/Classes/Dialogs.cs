using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace RepsCore.ViewModels.Classes
{

    /// <summary>
    /// IO Dialog Service
    /// </summary>
    #region == IO Dialog Serviceダイアログ表示用クラス ==

    /// TODO: サービスのインジェクションは・・・とりあえずしない。
    /// https://stackoverflow.com/questions/28707039/trying-to-understand-using-a-service-to-open-a-dialog?noredirect=1&lq=1
    /*
    public interface IOpenDialogService
    {
        string[] GetOpenPictureFileDialog(string title, bool multi = true);
    }
    */

    public class OpenDialogService// : IOpenDialogService
    {

        public string[] GetOpenPictureFileDialog(string title, bool multi = true)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = multi;
            openFileDialog.Filter = "イメージファイル (*.jpg;*.png;*.gif;*.jpeg)|*.png;*.jpg;*.gif;*.jpeg|写真ファイル (*.jpg;*.png;*.jpeg)|*.jpg;*.png;*.jpeg|画像ファイル(*.gif;*.png)|*.gif;*.png"; // 外観ならJPGかPNGのみ。間取りならGIFかPNG。
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures); // or MyDocuments
            openFileDialog.Title = title;

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileNames;
            }
            return null;
        }

        public string GetOpenZumenPdfFileDialog(string title)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "PDFファイル (*.pdf)|*.pdf";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.Title = title;

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }
            return null;
        }
    }

    #endregion

}
