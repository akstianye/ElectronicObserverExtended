using ElectronicObserver.Utility.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ElectronicObserver.Resource.Record
{

	/// <summary>
	/// レコードの基底です。
	/// </summary>
	public abstract class RecordBase
	{


		/// <summary>
		/// レコードの要素の基底です。
		/// </summary>
		public abstract class RecordElementBase
		{

			public abstract void LoadLine(string line);
			public abstract string SaveLine();

		}



		/// <summary>
		/// ファイルからレコードを読み込みます。
		/// </summary>
		/// <param name="path">ファイルが存在するフォルダのパス。</param>
		public virtual bool Load(string path)
		{

			path = GetFilePath(path);

			try
			{
				bool hasError = false;

				using (StreamReader sr = new StreamReader(path, Utility.Configuration.Config.Log.FileEncoding))
				{

					ClearRecord();

					bool ignoreError = false;

					string line;
					sr.ReadLine();          //ヘッダを読み飛ばす

					while ((line = sr.ReadLine()) != null)
					{
						try
						{
							LoadLine(line);

						}
						catch (Exception ex)
						{
							if (ignoreError)
								continue;

							hasError = true;
							Utility.ErrorReporter.SendErrorReport(ex, $"レコード {Path.GetFileName(path)} の破損を検出しました。");

							switch (MessageBox.Show($"记录 {Path.GetFileName(path)} 检测出损坏的数据\r\n\r\n[中止]: 停止读取。可能会丢失数据。。\r\n[重试]: <推荐>继续读取。\r\n[無視]: 继续读取。(以后不再确认。)",
								"记录损坏检测", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2))
							{
								case DialogResult.Abort:
									throw;

								case DialogResult.Retry:
									// do nothing
									break;

								case DialogResult.Ignore:
									ignoreError = true;
									break;
							}
						}
					}

				}

				if (hasError)
				{
					string backupDestination = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + "_backup_" + DateTimeHelper.GetTimeStamp() + Path.GetExtension(path));
					File.Copy(path, backupDestination);
					Utility.Logger.Add(3, $"修复以前的数据 {backupDestination} 备份在这。恢复失败的话请尝试从这个文件恢复。");

					SaveAll(RecordManager.Instance.MasterPath);
				}

				UpdateLastSavedIndex();
				return true;

			}
			catch (FileNotFoundException)
			{
				Utility.Logger.Add(1, "记录 " + path + " 不存在。");

			}
			catch (Exception ex)
			{
				Utility.ErrorReporter.SendErrorReport(ex, "记录 " + path + " 读取失败。");
			}

			return false;
		}


		/// <summary>
		/// ファイルに全てのレコードを書き込みます。
		/// </summary>
		/// <param name="path">ファイルが存在するフォルダのパス。</param>
		public virtual bool SaveAll(string path)
		{

			path = GetFilePath(path);

			try
			{

				using (StreamWriter sw = new StreamWriter(path, false, Utility.Configuration.Config.Log.FileEncoding))
				{

					sw.WriteLine(RecordHeader);
					sw.Write(SaveLinesAll());

				}

				UpdateLastSavedIndex();
				return true;

			}
			catch (Exception ex)
			{
				Utility.ErrorReporter.SendErrorReport(ex, "记录 " + path + " 读取失败。");
			}

			return false;
		}


		/// <summary>
		/// ファイルに前回からの差分を追記します。
		/// </summary>
		/// <param name="path">ファイルが存在するフォルダのパス。</param>
		public virtual bool SavePartial(string path)
		{

			if (!SupportsPartialSave)
				return false;


			path = GetFilePath(path);
			bool exists = File.Exists(path);

			try
			{

				using (StreamWriter sw = new StreamWriter(path, true, Utility.Configuration.Config.Log.FileEncoding))
				{

					if (!exists)
						sw.WriteLine(RecordHeader);

					sw.Write(SaveLinesPartial());
				}

				UpdateLastSavedIndex();
				return true;

			}
			catch (Exception ex)
			{
				Utility.ErrorReporter.SendErrorReport(ex, "记录 " + path + " 写入失败。");
			}

			return false;
		}


		protected string GetFilePath(string path)
		{
			return path.Trim(@" \\""".ToCharArray()) + "\\" + FileName;
		}


		/// <summary>
		/// ファイルから読み込んだデータを解析し、レコードに追加します。
		/// </summary>
		/// <param name="line">読み込んだ一行分のデータ。</param>
		protected abstract void LoadLine(string line);

		/// <summary>
		/// レコードのデータをファイルに書き込める文字列に変換します。
		/// </summary>
		protected abstract string SaveLinesAll();

		/// <summary>
		/// レコードの差分データをファイルに書き込める文字列に変換します。
		/// </summary>
		protected abstract string SaveLinesPartial();

		public abstract bool SupportsPartialSave { get; }


		/// <summary>
		/// レコードをクリアします。ロード直前に呼ばれます。
		/// </summary>
		protected abstract void ClearRecord();

		protected abstract void UpdateLastSavedIndex();

		public abstract bool NeedToSave { get; }


		public abstract void RegisterEvents();


		/// <summary>
		/// レコードのヘッダを取得します。
		/// </summary>
		public abstract string RecordHeader { get; }

		/// <summary>
		/// 保存するファイル名を取得します。
		/// </summary>
		public abstract string FileName { get; }

	}

}
