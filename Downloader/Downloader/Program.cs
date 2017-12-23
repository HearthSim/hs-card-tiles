using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Resources;
using HearthDb;
using HearthDb.Enums;
using nQuant;

namespace Downloader
{
	public class Program
	{
		private static WebClient _webClient;
		private static WebClient WebClient => _webClient ?? (_webClient = new WebClient());

		private static void Main(string[] args)
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
													| SecurityProtocolType.Tls11
													| SecurityProtocolType.Tls;
			var dir = Path.Combine(args[0], "Tiles");
			Directory.CreateDirectory(dir);
			foreach(var card in Cards.All.Values)
			{
				if(card.Set == CardSet.CHEAT)
					continue;
				if(!card.Collectible && card.Type != CardType.MINION
					&& card.Type != CardType.SPELL && card.Type != CardType.WEAPON)
					continue;
				var img = new FileInfo($"{dir}\\{card.Id}.png");
				if(!img.Exists)
					DownloadTile(card, img);
			}
		}

		private static void DownloadTile(Card card, FileInfo img)
		{
			Console.WriteLine($"Downloading missing image data for {card.Name} ({card.Id})");
			try
			{
				var data = WebClient.DownloadData($"https://art.hearthstonejson.com/v1/tiles/{card.Id}.png");
				Bitmap src;
				using(var ms = new MemoryStream(data))
					src = new Bitmap(new Bitmap(ms), 148, 34);
				var crop = new Rectangle(0, 0, 130, 34);
				var target = new Bitmap(crop.Width, crop.Height);
				using(var g = Graphics.FromImage(target))
					g.DrawImage(src, crop, new Rectangle(src.Width - crop.Width - 4, 0, crop.Width, src.Height), GraphicsUnit.Pixel);

				var quantizer = new WuQuantizer();
				using(var quantized = quantizer.QuantizeImage(target, 0, 0))
					quantized.Save(img.FullName, ImageFormat.Png);
			}
			catch(WebException ex)
			{
				Console.WriteLine("Error! " + ex.Message);
			}

		}
	}
}
