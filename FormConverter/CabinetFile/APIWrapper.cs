using System;
using System.Runtime.InteropServices;

namespace CabinetFile
{
	/// <summary>
	/// The FILE_IN_CABINET_INFO class provides information about a file found in the cabinet.
	/// Platform SDK: Setup API 
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
  	public class FILE_IN_CABINET_INFO
	{
		public String	NameInCabinet;
		public uint		FileSize;  
		public uint		Win32Error;  
		public ushort	DosDate;  
		public ushort	DosTime;  
		public ushort	DosAttribs;  
		
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)]
		public System.String	FullTargetName;
	}
	
	/// <summary>
	/// The FILEPATHS structure stores source and target path information. 
	/// Platform SDK: Setup API 
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
	public class FILEPATHS
	{
		public String Target;  
		public String  Source;  
		public uint Win32Error;  
		public uint  Flags;
	} 

	/// <summary>
	/// The FILETIME structure is a 64-bit value representing the number of 100-nanosecond intervals since January 1, 1601 (UTC).
	/// Platform SDK: Windows System Information 
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
	public class FILETIME
	{
		public uint dwLowDateTime;  
		public uint dwHighDateTime;
	} 

	/// <summary>
	/// The SYSTEMTIME structure represents a date and time using individual members for the month, day, year, weekday, hour, minute, second, and millisecond.
	/// Platform SDK: Windows System Information
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
	public class SYSTEMTIME
	{
		public ushort wYear;  
		public ushort wMonth;  
		public ushort wDayOfWeek;
		public ushort wDay;  
		public ushort wHour;  
		public ushort wMinute;  
		public ushort wSecond;  
		public ushort wMilliseconds;  
	} 

	
	public enum FILEOP:uint
	{
		FILEOP_ABORT=0, // Abort cabinet processing.
		FILEOP_DOIT,	// Extract the current file.
		FILEOP_SKIP		// Skip the current file.
	}

	public enum SetupIterateCabinetAction:uint
	{
		Iterate=0, Extract 
	}
	
	/// <summary>
	/// The FileCallback callback function is used by a number of the setup functions. The PSP_FILE_CALLBACK type defines a pointer to this callback function. FileCallback is a placeholder for the application-defined function name.
	/// Platform SDK: Setup API
	/// </summary>
	public delegate uint PSP_FILE_CALLBACK(uint context, uint notification, IntPtr param1, IntPtr param2);
	
	/// <summary>
	/// SetupApi Wrapper class
	/// Platform SDK: Setup API
	/// </summary>
	public class SetupApiWrapper
	{
		public const uint SPFILENOTIFY_FILEINCABINET      = 0x00000011;	// The file has been extracted from the cabinet.
		public const uint SPFILENOTIFY_NEEDNEWCABINET     = 0x00000012;	// file is encountered in the cabinet.
		public const uint SPFILENOTIFY_FILEEXTRACTED      = 0x00000013;	// The current file is continued in the next cabinet.
		public const uint NO_ERROR     = 0;

		/// <summary>
		/// The SetupIterateCabinet function iterates through all the files in a cabinet and sends a notification to a callback function for each file found.
		/// Platform SDK: Setup API
		/// </summary>
		[DllImport("SetupApi.dll", CharSet=CharSet.Auto)]
		public static extern bool SetupIterateCabinet(string cabinetFile, uint reserved, PSP_FILE_CALLBACK callBack, uint context);			

	}

	public class KernelApiWrapper
	{
		[DllImport("Kernel32.dll", CharSet=CharSet.Auto)]
		public static extern uint GetLastError();
	}

	public class DateTimeConvert
	{
		[DllImport("Kernel32.dll", CharSet=CharSet.Auto)]
		public static extern uint GetLastError();

		[DllImport("Kernel32.dll", CharSet=CharSet.Auto)]
		public static extern bool DosDateTimeToFileTime (ushort wFatDate, ushort wFatTime, FILETIME lpFileTime );

		[DllImport("Kernel32.dll", CharSet=CharSet.Auto)]
		public static extern bool FileTimeToLocalFileTime (FILETIME lpFileTime, FILETIME lpLocalFileTime );

		[DllImport("Kernel32.dll", CharSet=CharSet.Auto)]
		public static extern bool FileTimeToSystemTime (FILETIME lpFileTime, SYSTEMTIME lpSystemTime );

	}

}
