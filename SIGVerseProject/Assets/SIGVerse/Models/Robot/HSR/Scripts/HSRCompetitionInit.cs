using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SIGVerse.ToyotaHSR;

namespace SIGVerse.Competition
{
	public class HSRCompetitionInit : MonoBehaviour
	{
		private const string FileName = "/../SIGVerseConfig/TeamLogo.jpg";

		private string teamLogoPath;

		public MeshRenderer teamLogoRenderer;

		void Awake()
		{
			this.teamLogoPath = Application.dataPath + FileName;
		}

		// Use this for initialization
		void Start()
		{
			Texture texture = this.CreateTextureFromImageFile(teamLogoPath);

			if(texture!=null)
			{
				this.teamLogoRenderer.material.mainTexture = texture;
			}
			else
			{
				this.teamLogoRenderer.gameObject.SetActive(false);
			}
		}

		// Update is called once per frame
		void Update()
		{
		}

		private Texture CreateTextureFromImageFile(string path)
		{
			try
			{
				byte[] imageBinary = this.ReadImageFile(path);

				Texture2D texture = new Texture2D(1, 1);

				texture.LoadImage(imageBinary);

				return texture;
			}
			catch (Exception exception)
			{
				SIGVerse.Common.SIGVerseLogger.Error("Couldn't open TeamLogo.jpg. msg="+exception.Message);
				return null;
			}
		}

		private byte[] ReadImageFile(string path)
		{
			FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			BinaryReader binaryReader = new BinaryReader(fileStream);
			byte[] values = binaryReader.ReadBytes((int)binaryReader.BaseStream.Length);

			binaryReader.Close();
			fileStream.Close();

			return values;
		}
	}
}
