using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace CabinetFile
{
	/// <summary>
	/// Represent file in cabinet file
	/// </summary>
	public class TFile 
	{
		public TFile(FILE_IN_CABINET_INFO fileInCabinetInfo)
		{
			this.Name = System.IO.Path.GetFileName(fileInCabinetInfo.NameInCabinet);
			this.Path = System.IO.Path.GetDirectoryName(fileInCabinetInfo.NameInCabinet);
			this.Size = fileInCabinetInfo.FileSize;

			this.Date = GetDateTime(fileInCabinetInfo.DosDate, fileInCabinetInfo.DosTime);
		}

		public TFile()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// file name
		/// </summary>
		private string m_Name = "";
		public string Name
		{
			get { return m_Name;}
			set { m_Name = value;}
		}
        /// <summary>
        /// last modifed time
        /// </summary>
		private System.DateTime m_Date;      
		public System.DateTime Date
		{
			get { return m_Date;}
			set { m_Date = value;}
		}
      
		/// <summary>
		/// size of file
		/// </summary>
		private uint m_Size = 0;
		public uint Size
		{
			get { return m_Size;}
			set { m_Size = value;}
		}
        
		/// <summary>
		/// file path in cabinet
		/// </summary>
		private string m_Path = "";
		public string Path
		{
			get { return m_Path;}
			set { m_Path = value;}
		}

		/// <summary>
		/// full path of file in cabinet
		/// </summary>
		public string FullName
		{
			get { return this.Path + System.IO.Path.DirectorySeparatorChar.ToString() + this.Name;}
		}


		public override string ToString()
		{
			return FullName;
		}

		public override int GetHashCode()
		{
			return (this.Size > int.MaxValue ? int.MaxValue : (int)this.Size );
		}

		public override bool Equals(object obj)
		{
			if (obj==null) return false;
			if (this.GetType() != obj.GetType()) return false;

			TFile other = (TFile)obj;
			return ( string.Compare(this.Path, other.Path, true) == 0 && string.Compare(this.Name, other.Name, true)==0);
		}

		public static bool operator ==(TFile a, TFile b)
		{
			if ((object)a == null) return false;
			return a.Equals(b);
		}

		public static bool operator !=(TFile a, TFile b)
		{
			return !(a==b);
		}

		private DateTime GetDateTime(ushort date, ushort time)
		{
			FILETIME lpFileTime = new FILETIME();
			SYSTEMTIME  lpSystemTime = new SYSTEMTIME();
			DateTimeConvert.DosDateTimeToFileTime(date, time, lpFileTime);
			DateTimeConvert.FileTimeToSystemTime(lpFileTime, lpSystemTime);

			DateTime dateTime = new DateTime(lpSystemTime.wYear, lpSystemTime.wMonth, lpSystemTime.wDay,
				lpSystemTime.wHour, lpSystemTime.wMinute, lpSystemTime.wSecond, lpSystemTime.wMilliseconds);

			return dateTime;
		}

	}
}
