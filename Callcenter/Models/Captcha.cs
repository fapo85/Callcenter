using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Callcenter.Models
{
    public class Captcha
    {
        [BsonId]
        public ObjectId id { get; set; }
        public string Secret { get; set; }
        public DateTime Timestamp { get; set; }

        public Captcha(ObjectId id, string Secret)
        {
            this.id = id;
            this.Secret = Secret;
            this.Timestamp = DateTime.Now;
        }

        public byte[] CaptchaByteData()
        {
            const int width = 130, hight = 50;
            using (Bitmap bitmap = new Bitmap(width, hight))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                Random random = new Random();
                graphics.Clear(ColorLight());
                AddLetters();
                AddLine();
                Effect();
                MemoryStream stream = new MemoryStream();
                bitmap.Save(stream, ImageFormat.Png);
                return stream.ToArray();
                Color ColorLight()
                {
                    const int low = 185;
                    int nRend = random.Next(255) % (255 - low) + low;
                    int nGreen = random.Next(255) % (255 - low) + low;
                    int nBlue = random.Next(255) % (255 - low) + low;

                    return Color.FromArgb(nRend, nGreen, nBlue);
                }
                Color ColorDeep()
                {
                    const int redlow = 160, greenLow = 100, blueLow = 160;
                    return Color.FromArgb(random.Next(redlow), random.Next(greenLow), random.Next(blueLow));
                }
                void AddLetters()
                {
                    SolidBrush brush = new SolidBrush(Color.Transparent);
                    int fontSize = width / CaptchaFactory.SECRETLENTH;
                    Font font = new Font(FontFamily.GenericMonospace, fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
                    for (int i = 0; i < CaptchaFactory.SECRETLENTH; i++)
                    {
                        brush.Color = ColorDeep();
                        int shift = fontSize / 6;
                        float x = i * fontSize + random.Next(-shift, shift) + random.Next(-shift, shift);
                        int maxY = hight - fontSize;
                        if (maxY < 0) maxY = 0;
                        float y = random.Next(0, maxY);
                        graphics.DrawString(Secret[i].ToString(), font, brush, x, y);
                    }
                }
                void Effect()
                {
                    short nWave = 6;
                    int nWidth = bitmap.Width;
                    int nHeight = bitmap.Height;
                    Point[,] pt = new Point[nWidth, nHeight];
                    for (int x = 0; x < nWidth; ++x)
                    {
                        for (int y = 0; y < nHeight; ++y)
                        {
                            var xo = nWave * Math.Sin(2.0 * 3.1415 * y / 128.0);
                            var yo = nWave * Math.Cos(2.0 * 3.1415 * x / 128.0);
                            var newX = x + xo;
                            var newY = y + yo;
                            if (newX > 0 && newX < nWidth)
                            {
                                pt[x, y].X = (int)newX;
                            }
                            else
                            {
                                pt[x, y].X = 0;
                            }
                            if (newY > 0 && newY < nHeight)
                            {
                                pt[x, y].Y = (int)newY;
                            }
                            else
                            {
                                pt[x, y].Y = 0;
                            }
                        }
                    }
                    Bitmap clone = (Bitmap)bitmap.Clone();
                    BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                    BitmapData bmSrc = clone.LockBits(new Rectangle(0, 0, clone.Width, clone.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                    int scanline = bitmapData.Stride;
                    IntPtr scan0 = bitmapData.Scan0;
                    IntPtr srcScan0 = bmSrc.Scan0;
                    unsafe
                    {
                        byte* p = (byte*)(void*)scan0;
                        byte* pSrc = (byte*)(void*)srcScan0;
                        int nOffset = bitmapData.Stride - bitmap.Width * 3;
                        for (int y = 0; y < nHeight; ++y)
                        {
                            for (int x = 0; x < nWidth; ++x)
                            {
                                var xOffset = pt[x, y].X;
                                var yOffset = pt[x, y].Y;
                                if (yOffset >= 0 && yOffset < nHeight && xOffset >= 0 && xOffset < nWidth)
                                {
                                    if (pSrc != null)
                                    {
                                        p[0] = pSrc[yOffset * scanline + xOffset * 3];
                                        p[1] = pSrc[yOffset * scanline + xOffset * 3 + 1];
                                        p[2] = pSrc[yOffset * scanline + xOffset * 3 + 2];
                                    }
                                }
                                p += 3;
                            }
                            p += nOffset;
                        }
                    }
                    bitmap.UnlockBits(bitmapData);
                    clone.UnlockBits(bmSrc);
                    clone.Dispose();
                }
                void AddLine()
                {
                    Pen pen = new Pen(new SolidBrush(Color.Transparent), 3);
                    for (int i = 0; i < random.Next(3, 5); i++)
                    {
                        pen.Color = ColorDeep();
                        Point start = new Point(random.Next(0, width), random.Next(0, hight));
                        Point end = new Point(random.Next(0, width), random.Next(0, hight));
                        graphics.DrawLine(pen, start, end);
                    }
                }
            }
        }
    }
}