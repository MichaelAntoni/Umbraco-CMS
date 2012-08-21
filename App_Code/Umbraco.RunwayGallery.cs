using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Collections;

using umbraco.interfaces;
using umbraco.cms.businesslogic.web;
using ICSharpCode.SharpZipLib.Zip;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Umbraco.RunwayGallery
{
    public class ZipUploadHandler : umbraco.BusinessLogic.ApplicationBase
    {
        public ZipUploadHandler()
        {
            umbraco.content.AfterUpdateDocumentCache += new umbraco.content.DocumentCacheEventHandler(content_AfterUpdateDocumentCache);

        }

        void content_AfterUpdateDocumentCache(Document sender, umbraco.cms.businesslogic.DocumentCacheEventArgs e)
        {
            if (sender.ContentType.Alias == "runwayGalleryAlbum")
            {
                if (sender.getProperty("zipBulkUpload") != null && !String.IsNullOrEmpty(sender.getProperty("zipBulkUpload").Value.ToString()))
                {
                    string zipFile = umbraco.GlobalSettings.FullpathToRoot + sender.getProperty("zipBulkUpload").Value.ToString();

                    // Loop through and extract all images
                    string zipDir = unpackZip(zipFile);

                    foreach (string file in Directory.GetFiles(zipDir))
                    {
                        createMedia(file, sender);
                    }

                    // Delete zipped files
                    Directory.Delete(zipDir);

                    // Remove property
                    sender.getProperty("zipBulkUpload").Value = "";
                }
            }
        }

        private void createMedia(string filePath, Document parent)
        {

            // this method contains redundant code from the upload datatype as it's not possible
            // to access the methods used by the upload datatype to generate thumbnails and update 
            // image sizes

            FileInfo imageFile = new FileInfo(filePath);
            if (umbraco.UmbracoSettings.ImageFileTypes.Contains(imageFile.Extension.Replace(".", "")))
            {
                Document image = Document.MakeNew(umbraco.helper.SpaceCamelCasing(imageFile.Name.Replace(imageFile.Extension, "")), DocumentType.GetByAlias("RunwayGalleryPhoto"), parent.User, parent.Id);

                // move file
                string imagePropertyId = image.getProperty("umbracoFile").Id.ToString();
                string filename = "";
                string fullFilePath = "";
                if (umbraco.UmbracoSettings.UploadAllowDirectories)
                {
                    // Create a new folder in the /media folder with the name /media/propertyid
                    System.IO.Directory.CreateDirectory(System.Web.HttpContext.Current.Server.MapPath(umbraco.GlobalSettings.Path + "/../media/" + imagePropertyId));
                    fullFilePath = System.Web.HttpContext.Current.Server.MapPath(umbraco.GlobalSettings.Path + "/../media/" + imagePropertyId + "/" + imageFile.Name);
                    File.Move(filePath, fullFilePath);
                    filename = "/media/" + imagePropertyId + "/" + imageFile.Name;
                    image.getProperty("umbracoFile").Value = filename;
                }
                else
                {
                    filename = imagePropertyId + "-" + filename;
                    fullFilePath = System.Web.HttpContext.Current.Server.MapPath(umbraco.GlobalSettings.Path + "/../media/" + filename);
                    File.Move(filePath, fullFilePath);
                    image.getProperty("umbracoFile").Value = "/media/" + filename;
                }

                imageFile = new FileInfo(fullFilePath);

                // save extension
                image.getProperty("umbracoExtension").Value = imageFile.Extension.Replace(".", "");

                // save size
                image.getProperty("umbracoBytes").Value = imageFile.Length.ToString();

                // generate thumbnails
                generateThumbnails(image, imageFile);

                // publish image
                image.Publish(image.User);
                umbraco.library.PublishSingleNode(image.Id);

            }
        }

        private void generateThumbnails(Document imageDocument, FileInfo imageFile)
        {
            int fileWidth;
            int fileHeight;

            FileStream fs = new FileStream(imageFile.FullName,
                FileMode.Open, FileAccess.Read, FileShare.Read);

            System.Drawing.Image image = System.Drawing.Image.FromStream(fs);
            fileWidth = image.Width;
            fileHeight = image.Height;
            fs.Close();
            try
            {
                imageDocument.getProperty("umbracoWidth").Value = fileWidth.ToString();
                imageDocument.getProperty("umbracoHeight").Value = fileHeight.ToString();
            }
            catch { }

            // Generate thumbnails
            string fileNameThumb = imageFile.FullName.Replace(imageFile.Extension, "_thumb");
            generateThumbnail(image, 100, fileWidth, fileHeight, imageFile.FullName, imageFile.Extension.Replace(".", ""), fileNameThumb + ".jpg");
            SortedList thumbnailSizesList = umbraco.cms.businesslogic.datatype.PreValues.GetPreValues(imageDocument.getProperty("umbracoFile").PropertyType.DataTypeDefinition.Id);
            string thumbnails = "";
            if (thumbnailSizesList.Count > 0)
            {
                string[] thumbnailSizes = thumbnailSizesList[0].ToString().Split(";".ToCharArray());
                foreach (string thumb in thumbnailSizes)
                    if (thumb != "")
                        generateThumbnail(image, int.Parse(thumb), fileWidth, fileHeight, imageFile.FullName, imageFile.Extension.Replace(".", ""), fileNameThumb + "_" + thumb + ".jpg");
            }

            image.Dispose();
        }


        private void generateThumbnail(System.Drawing.Image image, int maxWidthHeight, int fileWidth, int fileHeight, string fullFilePath, string ext, string thumbnailFileName)
        {
            // Generate thumbnail
            float fx = (float)fileWidth / (float)maxWidthHeight;
            float fy = (float)fileHeight / (float)maxWidthHeight;
            // must fit in thumbnail size
            float f = Math.Max(fx, fy); //if (f < 1) f = 1;
            int widthTh = (int)Math.Round((float)fileWidth / f); int heightTh = (int)Math.Round((float)fileHeight / f);

            // fixes for empty width or height
            if (widthTh == 0)
                widthTh = 1;
            if (heightTh == 0)
                heightTh = 1;

            // Create new image with best quality settings
            
            Bitmap bp = new Bitmap(widthTh, heightTh);
            Graphics g = Graphics.FromImage(bp);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // Copy the old image to the new and resized
            Rectangle rect = new Rectangle(0, 0, widthTh, heightTh);
            g.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);

            // Copy metadata
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo codec = null;
            for (int i = 0; i < codecs.Length; i++)
            {
                if (codecs[i].MimeType.Equals("image/jpeg"))
                    codec = codecs[i];
            }

            // Set compresion ratio to 90%
            EncoderParameters ep = new EncoderParameters();
            ep.Param[0] = new EncoderParameter(Encoder.Quality, 90L);

            // Save the new image
            bp.Save(thumbnailFileName, codec, ep);
            bp.Dispose();
            g.Dispose();

        }

        private string unpackZip(string zipFile)
        {
            string tempDir = HttpContext.Current.Server.MapPath(umbraco.GlobalSettings.StorageDirectory) + Path.DirectorySeparatorChar + Guid.NewGuid().ToString();
            Directory.CreateDirectory(tempDir);

            ZipInputStream s = new ZipInputStream(File.OpenRead(zipFile));

            ZipEntry theEntry;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string directoryName = Path.GetDirectoryName(theEntry.Name);
                string fileName = Path.GetFileName(theEntry.Name);

                if (fileName != String.Empty)
                {
                    FileStream streamWriter = File.Create(tempDir + Path.DirectorySeparatorChar + fileName);

                    int size = 2048;
                    byte[] data = new byte[2048];
                    while (true)
                    {
                        size = s.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            streamWriter.Write(data, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }

                    streamWriter.Close();

                }
            }

            // Clean up
            s.Close();
            File.Delete(zipFile);

            return tempDir;
        }
    }
}
