using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace MySqlTool.Class
{
	public class SerializeHelper
	{
		public static ResultMessage Serialize(string filename, object obj)
		{
			ResultMessage resultMessage = new ResultMessage();
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				FileStream fileStream = new FileStream(filename, FileMode.Create);
				binaryFormatter.Serialize(fileStream, obj);
				fileStream.Close();
				fileStream.Dispose();
				resultMessage.Result = true;
			}
			catch (Exception ex)
			{
				resultMessage.Result = false;
				resultMessage.Message = ex.Message;
			}
			return resultMessage;
		}

		public static ResultMessage Deserialize(string filename)
		{
			ResultMessage resultMessage = new ResultMessage();
			try
			{
				if (!File.Exists(filename))
				{
					resultMessage.Result = false;
					resultMessage.Message = "文件不存在";
				}
				else
				{
					BinaryFormatter binaryFormatter = new BinaryFormatter();
					FileStream fileStream = new FileStream(filename, FileMode.Open);
					resultMessage.ObjResult = binaryFormatter.Deserialize(fileStream);
					fileStream.Close();
					fileStream.Dispose();
					resultMessage.Result = true;
				}
			}
			catch (Exception ex)
			{
				resultMessage.Result = false;
				resultMessage.Message = ex.Message;
			}
			return resultMessage;
		}

		public static ResultMessage XmlSerialize(string filename, object obj)
		{
			ResultMessage resultMessage = new ResultMessage();
			try
			{
				XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());
				FileStream fileStream = new FileStream(filename, FileMode.Create);
				xmlSerializer.Serialize(fileStream, obj);
				fileStream.Close();
				fileStream.Dispose();
				resultMessage.Result = true;
			}
			catch (Exception ex)
			{
				resultMessage.Result = false;
				resultMessage.Message = ex.Message;
			}
			return resultMessage;
		}

		public static ResultMessage XmlDeserialize<T>(string filename)
		{
			ResultMessage resultMessage = new ResultMessage();
			try
			{
				if (!File.Exists(filename))
				{
					resultMessage.Result = false;
					resultMessage.Message = "文件不存在";
				}
				else
				{
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
					FileStream fileStream = new FileStream(filename, FileMode.Open);
					resultMessage.ObjResult = xmlSerializer.Deserialize(fileStream);
					fileStream.Close();
					fileStream.Dispose();
					resultMessage.Result = true;
				}
			}
			catch (Exception ex)
			{
				resultMessage.Result = false;
				resultMessage.Message = ex.Message;
			}
			return resultMessage;
		}
	}
}
