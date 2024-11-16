﻿using Android.OS;
using Android.Content;
using Woyou.Aidlservice.Jiuiv5;
using SunmiPOSLib.Services;
using SunmiPOSLib.Models;
using SunmiPOSLib.Exceptions;
using SunmiPOSLib.Utils;
using Android.Graphics;
using Image = SunmiPOSLib.Models.Image;
using Application = Android.App.Application;
using SunmiPOSLib.Enum;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Android.Graphics.Drawables;

namespace SunmiPOSLib;

public class PrinterConnection : IPrinterConnection
{
    private SunmiPrinterService SunmiPrinterService { get; set; }

    public PrinterConnection()
    {
        SunmiPrinterService = new SunmiPrinterService();
        InitConnection();
    }

    public void SendRawData(byte[] data)
    {
        try
        {
            SunmiPrinterService.Service.SendRAWData(data, null);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    public bool InitConnection()
    {
        try
        {
            Intent intent = new Intent();
            intent.SetPackage("woyou.aidlservice.jiuiv5");
            intent.SetAction("woyou.aidlservice.jiuiv5.IWoyouService");

            bool isBound = Android.App.Application.Context.BindService(intent, SunmiPrinterService, Bind.AutoCreate);
            Console.WriteLine("Service binding result: " + isBound);

            return isBound;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception during service binding: {ex.Message}");
            return false;
        }
    }



    public bool CloseConnection()
    {
        if (!IsConnected()) return true;
        SunmiPrinterService.Service = null;
        return true;
    }

    public bool IsConnected()
    {
        return SunmiPrinterService.Service != null;
    }

    public bool PrintBarcode(Barcode barcode)
    {
        if (!IsConnected()) throw new PrinterConnectionException();
        try
        {
            SunmiPrinterService.Service.SetFontSize(16, null);
            int position = barcode.HRIPosition;
            SunmiPrinterService.Service.SetAlignment((int)AlignmentEnum.LEFT, null);
            SendRawData(CommandUtils.GetBarcodeBytes(barcode));
            LineWrap();
            return true;
        }
        catch (Exception)
        {
            throw new PrintBarcodeException();
        }
    }

    public bool PrintQRCode(QRcode qrcode)
    {
        if (!IsConnected()) throw new PrinterConnectionException();
        try
        {
            SunmiPrinterService.Service.SetFontSize(16, null);

            SunmiPrinterService.Service.SetAlignment((int)qrcode.Alignment, null);
            SendRawData(CommandUtils.GetQrcodeBytes(qrcode));
            LineWrap();
            return true;
        }
        catch (Exception)
        {
            throw new PrintQrcodeException();
        }
    }

    public bool PrintBitmap(Image image)
    {
        if (!IsConnected()) throw new PrinterConnectionException();
        try
        {
            //SunmiPrinterService.Service.SetFontSize(16, null);

            //SunmiPrinterService.Service.SetAlignment((int)image.Alignment, null);

            //image.

            //SendRawData(CommandUtils.GetQrcodeBytes(image));
            LineWrap();
            return true;
        }
        catch (Exception)
        {
            throw new PrintImageException();
        }
    }    

    public bool PrintText(Text text)
    {

        if (!IsConnected()) throw new PrinterConnectionException();
        try
        {
            SendRawData(text.IsBold ? CommandUtils.BoldOn() : CommandUtils.BoldOff());
            SendRawData(text.IsUnderline ? CommandUtils.UnderlineWithOneDotWidthOn() : CommandUtils.UnderlineOff());
            SunmiPrinterService.Service.SetFontSize(text.TextSize, null);
            SunmiPrinterService.Service.PrintText(text.Content, null);
            SendRawData(CommandUtils.UnderlineOff());
            SendRawData(CommandUtils.BoldOff());
            LineWrap();
            return true;
        }
        catch (Exception ex)
        {
            throw new PrintTextException();
        }
    }

    public bool PrintImage(Image image)
    {
        if (!IsConnected()) throw new PrinterConnectionException();
        try
        {
            var context = Application.Context;

            SunmiPrinterService.Service.PrintText("Hello Sunmi world!" + "\n", null);
            SunmiPrinterService.Service.PrintText("--------------------------------\n", new SimpleCallback());

            SunmiPrinterService.Service.PrintTextWithFont("Hello Sunmi world!", "fonts/OpenSans-Semibold.ttf", 12, null);


            //PrintBarcode(new Barcode("21705070507", 2, 6, 70, false));
            //PrintQRCode(new QRcode("Hello Sunmi world!"));

            //var table = new Table(new string[] { "Name1", "Value" }, new int[] { 20, 20 }, new AlignmentEnum[] { AlignmentEnum.LEFT, AlignmentEnum.RIGHT });
            //PrintTable(table);
            //table = new Table(new string[] { "Name2", "Value" }, new int[] { 20, 20 }, new AlignmentEnum[] { AlignmentEnum.LEFT, AlignmentEnum.RIGHT });
            //PrintTable(table);
            //table = new Table(new string[] { "Name3", "Value333" }, new int[] { 20, 20 }, new AlignmentEnum[] { AlignmentEnum.LEFT, AlignmentEnum.RIGHT });
            //PrintTable(table);



            var printerVersion = SunmiPrinterService.Service.GetPrinterVersion();
            var test1 = SunmiPrinterService.Service.GetPrinterSerialNo();
            var test2 = SunmiPrinterService.Service.GetServiceVersion();
            var test3 = SunmiPrinterService.Service.GetFirmwareStatus();


            // Retrieve the resource ID for the image
            var resourceId = context.Resources.GetIdentifier(image.Resource, "drawable", context.PackageName);
            if (resourceId == 0)
            {
                Console.WriteLine($"Image resource '{image.Resource}' not found.");
                return false;
            }
            // Load the drawable and convert to Bitmap
            using var drawable = context.GetDrawable(resourceId) as BitmapDrawable;
            if (drawable == null || drawable.Bitmap == null)
            {
                Console.WriteLine("Failed to load drawable or bitmap is null.");
                return false;
            }
            
                //ConvertToPrintableBitmap(drawable.Bitmap);

            // Start buffered mode
            SunmiPrinterService.Service.EnterPrinterBuffer(true);
            var scaledBitmap = DownloadAndConvertToBWBitmap(@"https://smartstore.com/media/6430/pagebuilder/Story-Home-Counter-git-400px-stroke-40.png?size=120");


            // Use the SimpleCallback class as the ICallback implementation
            //var bmp1 = CreateSmileyBitmap();
            SunmiPrinterService.Service.PrintBitmap(scaledBitmap, new SimpleCallback());
            SunmiPrinterService.Service.PrintBitmapCustom(scaledBitmap, 1, null);
            
            //SunmiPrinterService.Service.PrintBitmap(bmp1, new SimpleCallback());
            //SunmiPrinterService.Service.PrintBitmapCustom(scaledBitmap, 1, null);
            //SunmiPrinterService.Service.PrintBitmap(bmp1, new SimpleCallback());
            //SunmiPrinterService.Service.PrintBitmapCustom(scaledBitmap, 1, null);
            ////SunmiPrinterService.Service.CommitPrinterBuffer();
            //SunmiPrinterService.Service.PrintBitmapCustom(bmp1, 1, null);
            ////SunmiPrinterService.Service.PrintBitmapCustom(CreateTestBitmap(), 2, null);
            ////SunmiPrinterService.Service.CommitPrinterBuffer();


            SunmiPrinterService.Service.ExitPrinterBuffer(true);

            //// Retrieve the resource ID for the image
            //var resourceId = context.Resources.GetIdentifier(image.Resource, "drawable", context.PackageName);
            //if (resourceId == 0)
            //{
            //    Console.WriteLine($"Image resource '{image.Resource}' not found.");
            //    return false;
            //}

            //// Load the drawable and convert to Bitmap
            //using var drawable = context.GetDrawable(resourceId) as BitmapDrawable;
            //if (drawable == null || drawable.Bitmap == null)
            //{
            //    Console.WriteLine("Failed to load drawable or bitmap is null.");
            //    return false;
            //}

            //// Convert and scale the Bitmap to fit the printer's maximum width
            //using var grayscaleBitmap = ConvertToGrayscaleAndScale(drawable.Bitmap, 384);

            //// Set alignment and print the Bitmap
            //SunmiPrinterService.Service.SetAlignment((int)image.Alignment, null);
            //SunmiPrinterService.Service.PrintBitmapCustom(grayscaleBitmap, 2, null);
            //SunmiPrinterService.Service.PrintBitmapCustom(CreateTestBitmap(), 2, null);

            // Optionally cut the paper if specified
            if (image.CutPaper) SendRawData(CommandUtils.CutPaper());

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception during image printing: {ex.Message}");
            throw new PrintImageException();
        }
    }


    public bool PrintImage_(Image image)
    {
        if (!IsConnected()) throw new PrinterConnectionException();
        try
        {
            var context = Application.Context;


            var printerVersion = SunmiPrinterService.Service.GetPrinterVersion();
            var test1 = SunmiPrinterService.Service.GetPrinterSerialNo();
            var test2 = SunmiPrinterService.Service.GetServiceVersion();
            var test3 = SunmiPrinterService.Service.GetFirmwareStatus();

            //SunmiPrinterService.Service.SetAlignment((int)image.Alignment, null);
            //SunmiPrinterService.Service.PrintText("Imagem\n", null);
            SunmiPrinterService.Service.PrintText(printerVersion + "\n", null);
            SunmiPrinterService.Service.PrintText("--------------------------------\n", new SimpleCallback());  

            // Use the SimpleCallback class as the ICallback implementation
            SunmiPrinterService.Service.PrintBitmap(CreateTestBitmap(), new SimpleCallback());

            SunmiPrinterService.Service.PrintBitmapCustom(CreateTestBitmap(), 2, null);


            // Retrieve the resource ID for the image
            var resourceId = context.Resources.GetIdentifier(image.Resource, "drawable", context.PackageName);
            if (resourceId == 0)
            {
                Console.WriteLine($"Image resource '{image.Resource}' not found.");
                return false;
            }

            // Load the drawable and convert to Bitmap
            using var drawable = context.GetDrawable(resourceId) as BitmapDrawable;
            if (drawable == null || drawable.Bitmap == null)
            {
                Console.WriteLine("Failed to load drawable or bitmap is null.");
                return false;
            }

            // Scale the Bitmap to fit the printer's maximum width
            using var scaledBitmap = ConvertToGrayscaleAndScale(drawable.Bitmap, 384);

            SunmiPrinterService.Service.PrintBitmapCustom(scaledBitmap, 2, null);







            byte[] imageData = ConvertBitmapToEscPos(CreateTestBitmap());

            // Send the raw data to the printer
            SunmiPrinterService.Service.SendRAWData(imageData, new SimpleCallback());


            //using (var drawable = Xamarin.Forms.Platform.Android.ResourceManager.GetDrawable(context, image.Resource))
            //using (var bitmap = ((BitmapDrawable)drawable).Bitmap)
            //{
            //    SunmiPrinterService.Service.PrintBitmap(ScaleImage(bitmap), null);
            //}
            LineWrap();
            if (image.CutPaper) SendRawData(CommandUtils.CutPaper());
            return true;
        }
        catch (Exception _)
        {
            throw new PrintImageException();
        }
    }
    private Bitmap ScaleBitmapToWidth(Bitmap original, int targetWidth)
    {
        int width = original.Width;
        int height = original.Height;

        if (width > targetWidth)
        {
            int scaledHeight = (int)(height * (float)targetWidth / width);
            return Bitmap.CreateScaledBitmap(original, targetWidth, scaledHeight, true);
        }
        return original;
    }

    private Bitmap ConvertToGrayscaleAndScale(Bitmap originalImage, int targetWidth)
    {
        int width = originalImage.Width;
        int height = originalImage.Height;

        if (width > targetWidth)
        {
            int scaledHeight = (int)(height * (float)targetWidth / width);
            originalImage = Bitmap.CreateScaledBitmap(originalImage, targetWidth, scaledHeight, true);
        }

        Bitmap grayscaleBitmap = Bitmap.CreateBitmap(originalImage.Width, originalImage.Height, Bitmap.Config.Argb8888);

        for (int y = 0; y < originalImage.Height; y++)
        {
            for (int x = 0; x < originalImage.Width; x++)
            {
                int pixel = originalImage.GetPixel(x, y);
                int gray = (int)(Android.Graphics.Color.GetRedComponent(pixel) * 0.3 +
                                 Android.Graphics.Color.GetGreenComponent(pixel) * 0.59 +
                                 Android.Graphics.Color.GetBlueComponent(pixel) * 0.11);
                int alpha = Android.Graphics.Color.GetAlphaComponent(pixel);
                grayscaleBitmap.SetPixel(x, y, Android.Graphics.Color.Argb(alpha, gray, gray, gray));
            }
        }

        return grayscaleBitmap;
    }

    public class SimpleCallback : Java.Lang.Object, ICallback
    {
        public IBinder? AsBinder() => null;

        public void OnRaiseException(int code, string msg)
        {
            Console.WriteLine($"Error Code: {code}, Message: {msg}");
        }

        public void OnReturnString(string result)
        {
            Console.WriteLine($"String Result: {result}");
        }

        public void OnRunResult(bool isSuccess)
        {
            Console.WriteLine(isSuccess ? "Operation Successful." : "Operation Failed.");
        }

        //public IBinder? AsBinder()
        //{
        //    throw new NotImplementedException();
        //}

        //public void OnRaiseException(int code, string msg)
        //{
        //    Console.WriteLine($"Exception raised. Code: {code}, Message: {msg}");
        //}

        //public void OnReturnString(string result)
        //{
        //    Console.WriteLine($"Return string from printer: {result}");
        //}

        //public void OnRunResult(bool isSuccess)
        //{
        //    if (isSuccess)
        //    {
        //        Console.WriteLine("Image printed successfully.");
        //    }
        //    else
        //    {
        //        Console.WriteLine("Failed to print image.");
        //    }
        //}
    }

    public byte[] ConvertBitmapToEscPos(Bitmap bitmap)
    {
        int width = bitmap.Width;
        int height = bitmap.Height;

        // ESC/POS command for starting raster mode
        List<byte> bytes = new List<byte> { 0x1D, 0x76, 0x30, 0x00, (byte)(width / 8), 0x00, (byte)(height % 256), (byte)(height / 256) };

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x += 8)
            {
                byte byteData = 0;

                for (int bit = 0; bit < 8; bit++)
                {
                    int pixelX = x + bit;
                    if (pixelX < width)
                    {
                        int pixelColor = bitmap.GetPixel(pixelX, y);
                        byte grayscale = (byte)((Android.Graphics.Color.GetRedComponent(pixelColor) * 0.3 +
                                                 Android.Graphics.Color.GetGreenComponent(pixelColor) * 0.59 +
                                                 Android.Graphics.Color.GetBlueComponent(pixelColor) * 0.11) > 128 ? 1 : 0);

                        byteData |= (byte)(grayscale << (7 - bit));
                    }
                }
                bytes.Add(byteData);
            }
        }

        return bytes.ToArray();
    }


    public Bitmap CreateTestBitmap_()
    {
        // Set dimensions for the test bitmap
        int width = 384; // Adjust width based on your printer's requirements
        int height = 384; // Adjust height as needed for a square grid of tiles

        // Create a blank bitmap with the specified width and height
        Bitmap bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);

        // Set up a canvas to draw on the bitmap
        Canvas canvas = new Canvas(bitmap);

        // Define the number of tiles in both directions
        int numberOfTiles = 16; // Number of tiles per row and column
        int tileSize = width / numberOfTiles; // Size of each tile

        // Paint for drawing
        Android.Graphics.Paint paint = new Android.Graphics.Paint();

        // Loop through each tile and draw black and white tiles alternately
        for (int row = 0; row < numberOfTiles; row++)
        {
            for (int col = 0; col < numberOfTiles; col++)
            {
                // Set the color: black for even sum of row and col, white for odd
                if ((row + col) % 2 == 0)
                {
                    paint.Color = Android.Graphics.Color.Black;
                }
                else
                {
                    paint.Color = Android.Graphics.Color.White;
                }

                // Draw the tile
                float left = col * tileSize;
                float top = row * tileSize;
                float right = left + tileSize;
                float bottom = top + tileSize;

                canvas.DrawRect(left, top, right, bottom, paint);
            }
        }

        return bitmap;
    }

    public Bitmap CreateTestBitmap()
    {

        // Set dimensions for the test bitmap
        int width = 384; // Adjust width based on your printer's requirements
        int height = 384; // Adjust height as needed for a square grid of tiles

        // Create a blank bitmap with the specified width and height
        Bitmap bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);

        // Set up a canvas to draw on the bitmap
        Canvas canvas = new Canvas(bitmap);

        // Define the number of tiles in both directions
        int numberOfTiles = 16; // Number of tiles per row and column
        int tileSize = width / numberOfTiles; // Size of each tile

        // Paint for drawing
        Android.Graphics.Paint paint = new Android.Graphics.Paint();

        // Define the number of gray shades to use
        int numberOfGrayShades = 4; // Adjust as needed for more/less gradient steps

        // Loop through each tile and draw black, white, or gray tiles
        for (int row = 0; row < numberOfTiles; row++)
        {
            for (int col = 0; col < numberOfTiles; col++)
            {
                // Calculate the color based on the row and column
                if ((row + col) % 2 == 0)
                {
                    paint.Color = Android.Graphics.Color.Black; // Black tiles for even sum
                }
                else
                {
                    // Determine which shade of gray to use based on the row
                    int grayIndex = (row / (numberOfTiles / numberOfGrayShades)) % numberOfGrayShades;
                    int grayValue = (255 / (numberOfGrayShades - 1)) * grayIndex;
                    paint.Color = Android.Graphics.Color.Rgb(grayValue, grayValue, grayValue);
                }

                // Draw the tile
                float left = col * tileSize;
                float top = row * tileSize;
                float right = left + tileSize;
                float bottom = top + tileSize;

                canvas.DrawRect(left, top, right, bottom, paint);
            }
        }

        return bitmap;
    }

    public Bitmap CreateSmileyBitmap()
    {
        // Set the dimensions for the bitmap
        int width = 384; // Width suitable for thermal printers
        int height = 384; // Height suitable for a square canvas

        // Create a blank bitmap with the specified width and height
        Bitmap smileyBitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);

        // Set up a canvas to draw on the bitmap
        Canvas canvas = new Canvas(smileyBitmap);

        // Create paint for drawing
        Android.Graphics.Paint paint = new Android.Graphics.Paint();
        paint.Color = Android.Graphics.Color.White; // Set background to white
        paint.SetStyle(Android.Graphics.Paint.Style.Fill);

        // Fill the background with white
        canvas.DrawRect(0, 0, width, height, paint);

        // Draw the face circle with white fill
        float centerX = width / 2;
        float centerY = height / 2;
        float faceRadius = width / 3;
        paint.Color = Android.Graphics.Color.White;
        paint.SetStyle(Android.Graphics.Paint.Style.Fill);
        canvas.DrawCircle(centerX, centerY, faceRadius, paint);

        // Draw the black border around the face circle
        paint.Color = Android.Graphics.Color.Black;
        paint.SetStyle(Android.Graphics.Paint.Style.Stroke);
        paint.StrokeWidth = 5; // Set stroke width for the border
        canvas.DrawCircle(centerX, centerY, faceRadius, paint);

        // Draw the eyes
        paint.SetStyle(Android.Graphics.Paint.Style.Fill); // Use fill for the eyes
        float eyeRadius = faceRadius / 10;
        float eyeOffsetX = faceRadius / 3;
        float eyeOffsetY = faceRadius / 3;
        canvas.DrawCircle(centerX - eyeOffsetX, centerY - eyeOffsetY, eyeRadius, paint);
        canvas.DrawCircle(centerX + eyeOffsetX, centerY - eyeOffsetY, eyeRadius, paint);

        // Draw the smile
        paint.SetStyle(Android.Graphics.Paint.Style.Stroke); // Use stroke for the smile
        float smileRadius = faceRadius / 1.5f;
        Android.Graphics.RectF smileRect = new Android.Graphics.RectF(centerX - smileRadius, centerY - smileRadius / 2, centerX + smileRadius, centerY + smileRadius / 2);
        canvas.DrawArc(smileRect, 20, 140, false, paint); // Adjusted angles for a clearer smile

        return smileyBitmap;
    }

    public Bitmap ConvertToBlackAndWhite(Bitmap inputBitmap)
    {
        // Set the target width for the thermal printer
        int targetWidth = 384;
        int originalWidth = inputBitmap.Width;
        int originalHeight = inputBitmap.Height;

        // Calculate the aspect ratio and the new height
        float aspectRatio = (float)originalHeight / originalWidth;
        int targetHeight = (int)(targetWidth * aspectRatio);

        // Scale the input bitmap to the target width and height
        Bitmap scaledBitmap = Bitmap.CreateScaledBitmap(inputBitmap, targetWidth, targetHeight, true);

        // Create a new bitmap for the black-and-white image
        Bitmap bwBitmap = Bitmap.CreateBitmap(targetWidth, targetHeight, Bitmap.Config.Argb8888);

        // Iterate through each pixel and convert to black or white
        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                // Get the pixel color from the scaled bitmap
                int pixel = scaledBitmap.GetPixel(x, y);

                // Convert the pixel to grayscale
                int gray = (int)(0.3 * Android.Graphics.Color.GetRedComponent(pixel) +
                                 0.59 * Android.Graphics.Color.GetGreenComponent(pixel) +
                                 0.11 * Android.Graphics.Color.GetBlueComponent(pixel));

                // Binarize the pixel based on a threshold value
                int threshold = 128; // You can adjust this threshold value as needed
                if (gray < threshold)
                {
                    bwBitmap.SetPixel(x, y, Android.Graphics.Color.Black);
                }
                else
                {
                    bwBitmap.SetPixel(x, y, Android.Graphics.Color.White);
                }
            }
        }

        return bwBitmap;
    }

    public Bitmap ConvertToPrintableBitmap(Bitmap inputBitmap)
    {
        // Set the target width for the thermal printer
        int targetWidth = 384;
        int originalWidth = inputBitmap.Width;
        int originalHeight = inputBitmap.Height;

        // Calculate the aspect ratio and the new height
        float aspectRatio = (float)originalHeight / originalWidth;
        int targetHeight = (int)(targetWidth * aspectRatio);

        // Scale the input bitmap to the target width and height
        Bitmap scaledBitmap = Bitmap.CreateScaledBitmap(inputBitmap, targetWidth, targetHeight, true);

        // Create a new bitmap for the black-and-white output
        Bitmap bwBitmap = Bitmap.CreateBitmap(targetWidth, targetHeight, Bitmap.Config.Argb8888);

        // Set up a simple dithering pattern (Floyd-Steinberg, for example)
        int[,] ditherMatrix = { { 0, 0, 7 }, { 3, 5, 1 } };
        int matrixHeight = ditherMatrix.GetLength(0);
        int matrixWidth = ditherMatrix.GetLength(1);
        int ditherFactor = 16;

        // Iterate through each pixel and convert to black or white with simple dithering
        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                // Get the pixel color from the scaled bitmap
                int pixel = scaledBitmap.GetPixel(x, y);

                // Convert the pixel to grayscale
                int gray = (int)(0.3 * Android.Graphics.Color.GetRedComponent(pixel) +
                                 0.59 * Android.Graphics.Color.GetGreenComponent(pixel) +
                                 0.11 * Android.Graphics.Color.GetBlueComponent(pixel));

                // Apply a simple threshold to convert to black or white
                int newColor = gray < 128 ? 0 : 255; // Basic thresholding

                // Set the new pixel color on the black-and-white bitmap
                bwBitmap.SetPixel(x, y, newColor == 0 ? Android.Graphics.Color.Black : Android.Graphics.Color.White);
            }
        }

        return bwBitmap;
    }

    public Bitmap DownloadAndConvertToBWBitmap(string imageUrl)
    {
        // Step 1: Download the image from the URL synchronously
        using (HttpClient httpClient = new HttpClient())
        {
            // Download the image data as a byte array
            byte[] imageData = httpClient.GetByteArrayAsync(imageUrl).Result;

            // Step 2: Decode the byte array into a Bitmap
            Bitmap originalBitmap = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);

            // Step 3: Convert the bitmap to a black-and-white format for printing
            return ConvertToPrintableBitmap(originalBitmap);
        }
    }

    public bool PrintTable(Table table)
    {
        if (!IsConnected()) throw new PrinterConnectionException();
        try
        {
            SunmiPrinterService.Service.SetFontSize(24, null);
            SunmiPrinterService.Service.PrintColumnsText(table.ColumnsText, table.ColumnsWidth, table.GetAlignmentsAsInteger(), null);
            LineWrap(1);
            return true;
        }
        catch (Exception)
        {
            throw new PrintTableException();
        }
    }
    public bool PrintReceiptWithQR(Text text,QRcode qrCode)
    {
        this.PrintText(text);
        this.PrintQRCode(qrCode);
        SunmiPrinterService.Service.PrintText("\n--------------------------------\n", null);
        return true;

    }
    public string ShowPrinterStatus()
    {
        if (!IsConnected()) return "Printer disconnected";
        string result = "Interface is too low to implement";
        try
        {
            int res = SunmiPrinterService.Service.UpdatePrinterState();
            switch (res)
            {
                case 1:
                    result = "Printer is working";
                    break;
                case 2:
                    result = "Printer found but still initializing";
                    break;
                case 3:
                    result = "Printer hardware interface is abnormal and needs to be reprinted";
                    break;
                case 4:
                    result = "Printer is out of paper";
                    break;
                case 5:
                    result = "Printer is overheating";
                    break;
                case 6:
                    result = "The printer cover is not closed";
                    break;
                case 7:
                    result = "Printer cutout is faulty";
                    break;
                case 8:
                    result = "Printer cutter is normal";
                    break;
                case 9:
                    result = "No black label paper found";
                    break;
                case 505:
                    result = "Printer does not exist";
                    break;
                default:
                    break;
            }
        }
        catch (RemoteException e)
        {
            e.PrintStackTrace();
            return null;
        }
        return result;
    }

    public bool PrintInvoices(List<Invoice> invoices)
    {
        if (!IsConnected()) return false;
        try
        {
            foreach (Invoice invoice in invoices)
            {
                SunmiPrinterService.Service.SetAlignment((int)AlignmentEnum.CENTER, null);
                SendRawData(CommandUtils.BoldOn());
                SunmiPrinterService.Service.PrintText(
                    String.Join("", invoice.Content.ToArray()),
                    null
                );
                LineWrap();
                SendRawData(CommandUtils.CutPaper());
            }
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    public bool PrintInvoicesWithQR(List<InvoiceWithQR> invoices)
    {
        if (!IsConnected()) return false;
        try
        {
            foreach (InvoiceWithQR invoice in invoices)
            {
                SunmiPrinterService.Service.SetAlignment((int)AlignmentEnum.CENTER, null);
                SendRawData(CommandUtils.BoldOff());
                SunmiPrinterService.Service.PrintText(String.Join("", invoice.Content.ToArray()),null);
                SunmiPrinterService.Service.SetFontSize(24, null);
                SunmiPrinterService.Service.SetAlignment((int)AlignmentEnum.CENTER, null);
                SendRawData(CommandUtils.GetQrcodeBytes(new QRcode(invoice.QRCode)));
                LineWrap();
                SendRawData(CommandUtils.CutPaper());
            }
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    public bool AdvancePaper()
    {
        if (!IsConnected()) throw new PrinterConnectionException();
        try
        {
            LineWrap();
            return true;
        }
        catch (Exception)
        {
            throw new AdvancePaperException();
        }
    }

    private void LineWrap(int lines = 3)
    {
        if (!IsConnected()) return;
        try
        {
            SunmiPrinterService.Service.LineWrap(lines, null);
        }
        catch (Exception)
        {
        }
    }

    public string GetPrinterSerialNo()
    {
        return IsConnected() ? SunmiPrinterService.Service.GetPrinterSerialNo() : string.Empty;
    }

    public string GetPrinterModel()
    {
        var model = SysProp.GetProp("ro.product.model");

        return model ?? string.Empty;
    }

    public string GetFirmwareVersion()
    {
        return IsConnected() ? SunmiPrinterService.Service.GetPrinterVersion() : string.Empty;
    }

    public string GetServiceVersion()
    {
        return IsConnected() ? SunmiPrinterService.Service.GetServiceVersion() : string.Empty;
    }

    public int GetPrinterPaper()
    {
        return 1;
    }

    public Task<string> GetPrintedLength()
    {
        var cb = new Callback();
        SunmiPrinterService.Service.GetPrintedLength(cb);
        return cb.Result.Task;
    }

    public string GetServiceVersionName()
    {
        var versionName = SysProp.GetProp("ro.version.sunmi_versionname");

        return versionName ?? string.Empty;
    }

    public string GetServiceVersionCode()
    {
        var packageInfo = Application.Context.ApplicationContext.PackageManager.GetPackageInfo("woyou.aidlservice.jiuiv5", 0);
        var versionCode = AndroidX.Core.Content.PM.PackageInfoCompat.GetLongVersionCode(packageInfo);

        return versionCode.ToString();
    }

    private Bitmap ScaleImage(Bitmap bitmap1)
    {
        int width = (int)(bitmap1.Width * 0.5);
        int height = (int)(bitmap1.Height * 0.5);
        return Bitmap.CreateScaledBitmap(bitmap1, width, height, false);
    }
}

public class SunmiPrinterService : Java.Lang.Object, IServiceConnection
{
    public IWoyouService Service { get; set; }

    public void OnServiceConnected(ComponentName name, IBinder service)
    {
        Console.WriteLine("Service connected.");
        Service = IWoyouServiceStub.AsInterface(service);
    }

    public void OnServiceDisconnected(ComponentName name)
    {
        Console.WriteLine("Service disconnected.");
        Service = null;
    }

}

class Callback : ICallbackStub
{
    public TaskCompletionSource<String> Result;

    public Callback()
    {
        Result = new TaskCompletionSource<string>();
    }

    public override void OnRunResult(bool isSuccess)
    {
        //throw new NotImplementedException();
    }

    public override void OnReturnString(string result)
    {
        Result.TrySetResult(result);
    }

    public override void OnRaiseException(int code, string msg)
    {
        //throw new NotImplementedException();
    }
}

static class SysProp
{
    // Lazy load the SystemProperties class
    private static readonly Lazy<Java.Lang.Class> _class =
        new Lazy<Java.Lang.Class>(() =>
            Java.Lang.Class.ForName("android.os.SystemProperties")
        );

    // Get the set method when we need it
    private static readonly Lazy<Java.Lang.Reflect.Method> _SetMethod =
        new Lazy<Java.Lang.Reflect.Method>(() =>
            _class.Value.GetDeclaredMethod("set",
                Java.Lang.Class.FromType(typeof(Java.Lang.String)),
                Java.Lang.Class.FromType(typeof(Java.Lang.String)))
            );

    // Get the get method when we need it
    private static readonly Lazy<Java.Lang.Reflect.Method> _GetMethod =
        new Lazy<Java.Lang.Reflect.Method>(() =>
            _class.Value.GetDeclaredMethod("get",
                Java.Lang.Class.FromType(typeof(Java.Lang.String)))
            );

    private static Java.Lang.Reflect.Method SetMethod
    {
        get { return _SetMethod.Value; }
    }
    private static Java.Lang.Reflect.Method GetMethod
    {
        get { return _GetMethod.Value; }
    }

    /// <summary>
    /// Calls the get method of the android.os.SystemProperties class
    /// </summary>
    /// <param name="PropertyName">The name of the system property to get the value for</param>
    /// <returns>The value of the specified property or null if it does not exists</returns>
    public static string GetProp(string PropertyName)
    {
        // Invoking a static method, first parameter is null
        var r = GetMethod.Invoke(null, new Java.Lang.String(PropertyName));

        return r.ToString();
    }

    /// <summary>
    /// Calls the set method of the android.os.SystemProperties class
    /// </summary>
    /// <param name="PropertyName">The name of the system property to get the value for</param>
    /// <param name="PropertyValue">The value to set for the system property</param>
    /// <returns>The previous value of the specified property or null if it does not exists</returns>
    public static string SetProp(string PropertyName, string PropertyValue)
    {
        // Invoking a static method, first parameter is null
        var r = SetMethod.Invoke(null,
            new Java.Lang.String(PropertyName),
            new Java.Lang.String(PropertyValue));

        return r.ToString();
    }

}
