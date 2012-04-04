using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace FormConverter.CabinetFile
{
	
	public class TCabinetFile : CollectionBase, IBindingList, IDisposable
	{
		/// <summary>
		/// The FileCallback callback function is used by a number of the setup functions. The PSP_FILE_CALLBACK type defines a pointer to this callback function. FileCallback is a placeholder for the application-defined function name.
		/// Platform SDK: Setup API
		/// </summary>
		private uint CallBack(uint context, uint notification, IntPtr param1, IntPtr param2)
		{
			uint rtnValue = SetupApiWrapper.NO_ERROR;
			switch (notification)
			{        
				case SetupApiWrapper.SPFILENOTIFY_FILEINCABINET:
					rtnValue = OnFileFound(context, notification, param1, param2);
					break;
				case SetupApiWrapper.SPFILENOTIFY_FILEEXTRACTED:
					rtnValue = OnFileExtractComplete(param1);
					break;
				case SetupApiWrapper.SPFILENOTIFY_NEEDNEWCABINET:
					rtnValue = SetupApiWrapper.NO_ERROR;
					break;
			}
			return rtnValue;
		}


		public TCabinetFile()
		{

		}

		public TCabinetFile(string sCabFileName)
		{
			this.Name = sCabFileName;
		}

        public TCabinetFile(Stream inStream)
        {
            this.m_useTmpFile = true;

            var fileName = Path.GetTempFileName();

            using (var file = File.Create(fileName))
            {
                inStream.CopyTo(file);
            }
            
            this.Name = fileName;

        }

        private bool m_useTmpFile;

	    /// <summary>
		/// Cabinet file name
		/// </summary>
		private string m_Name = "";
		public string Name
		{
			set 
			{ 
				if (!System.IO.File.Exists(value))
					throw new System.Exception("This CabFile doesn't exist");

				PSP_FILE_CALLBACK callback = new PSP_FILE_CALLBACK(this.CallBack);				
				uint setupIterateCabinetAction = (uint)SetupIterateCabinetAction.Iterate;
				if (!SetupApiWrapper.SetupIterateCabinet(value, 0, callback, setupIterateCabinetAction))
				{
                    throw new Win32Exception((int)KernelApiWrapper.GetLastError());
				}	
				m_Name = value;
			}
			get { return m_Name;}
		}
        
		/// <summary>
		/// output directory
		/// </summary>
		private string m_OutputDirectory = "";
		public string OutputDirectory
		{
			set 
			{
                if ( !Directory.Exists(value) )
                    Directory.CreateDirectory(value);

				m_OutputDirectory = value;
			}
			get 
			{
				if (m_OutputDirectory.Length <=0 )
					return System.IO.Directory.GetCurrentDirectory();
				return m_OutputDirectory;
			}
		}
        

		/// <summary>
		/// if true, extract file without path in cabinet
		/// </summary>
		private bool m_IgnoreInsidePath = false;
		public bool IgnoreInsidePath
		{
			set { m_IgnoreInsidePath = value;}
			get { return m_IgnoreInsidePath;}
		}

		
		#region Events
		private ListChangedEventHandler m_ListChanged;
		public event ListChangedEventHandler ListChanged
		{
			add
			{
				m_ListChanged += value;
			}
			remove
			{
				m_ListChanged -= value;
			}
		}

		private ListChangedEventArgs resetEvent = new ListChangedEventArgs(ListChangedType.Reset, -1);

		private EventHandler m_FileFound;
		public event EventHandler FileFound
		{
			add
			{
				m_FileFound += value;
			}
			remove
			{
				m_FileFound -= value;
			}
		}

		
		private EventHandler m_FileExtractBefore;
		public event EventHandler FileExtractBefore
		{
			add
			{
				m_FileExtractBefore += value;
			}
			remove
			{
				m_FileExtractBefore -= value;
			}
		}

		
		private EventHandler m_FileExtractComplete;
		public event EventHandler FileExtractComplete
		{
			add
			{
				m_FileExtractComplete += value;
			}
			remove
			{
				m_FileExtractComplete -= value;
			}
		}

		#endregion

		#region CollectionBase Method
		
		public int Add(TFile item)
		{
			throw new NotSupportedException();
			//	return (List.Add(item));			
		}

		public int IndexOf(TFile item)  
		{
			return( List.IndexOf( item ) );
		}

		public void Insert(int index, TFile item)  
		{
			throw new NotSupportedException();
			//			List.Insert(index, item);
		}

		public void Remove(TFile item)
		{
			throw new NotSupportedException();
			//			List.Remove(item);
		}

		
		public bool Contains(TFile item)  
		{
			return( List.Contains( item ) );
		}


		public TFile this[int index]
		{
			get	{	return (TFile)List[index] ;	}
			set	
			{	
				throw new NotSupportedException();
				//	List[index] = value;		
			}
		}

		public override string ToString()
		{
			return this.Name;
		}

		public override int GetHashCode()
		{
			return (this.Count);
		}

	    public override bool Equals(object obj)
		{
			if (obj==null) return false;
			if (this.GetType() != obj.GetType()) return false;

			TCabinetFile other = (TCabinetFile)obj;
			return (string.Compare(this.Name, other.Name, true)==0);
		}

		public static bool operator ==(TCabinetFile a, TCabinetFile b)
		{
			if ((object)a == null) return false;
			return a.Equals(b);
		}

		public static bool operator !=(TCabinetFile a, TCabinetFile b)
		{
			return !(a==b);
		}
		
		#endregion

		protected virtual uint OnFileFound(uint context, uint notification, IntPtr param1, IntPtr param2)
		{
			uint fileOperation = SetupApiWrapper.NO_ERROR;
			FILE_IN_CABINET_INFO fileInCabinetInfo = (FILE_IN_CABINET_INFO)Marshal.PtrToStructure(param1, typeof(FILE_IN_CABINET_INFO));
			TFile file = new TFile(fileInCabinetInfo);
			switch(context)
			{
				case (uint)SetupIterateCabinetAction.Iterate:
					List.Add(file);
					if (m_FileFound != null)
						m_FileFound(file, System.EventArgs.Empty);
					fileOperation = (uint)FILEOP.FILEOP_SKIP;
					break;

				case (uint)SetupIterateCabinetAction.Extract:
					if (this.m_ExtractAll)
						this.m_ExtractFileIndex = this.IndexOf(file);

					fileOperation = OnFileExtractBefore(file);
					if (fileOperation == (uint)FILEOP.FILEOP_DOIT)
					{
						fileInCabinetInfo.FullTargetName = MakeExtractFileName(file);
						Marshal.StructureToPtr(fileInCabinetInfo, param1, true);
					}
					break;
			}
			return fileOperation;	
		}

		protected virtual uint OnFileExtractBefore(TFile file)
		{
			bool cancel = false;

			if (file != (TFile)this.List[m_ExtractFileIndex])
				return (uint)FILEOP.FILEOP_SKIP;

			if (m_FileExtractBefore != null)
			{
				System.ComponentModel.CancelEventArgs arg = new CancelEventArgs(false);
				m_FileExtractBefore(file, arg);
				cancel = arg.Cancel;
			}
			if (cancel)  
				return (uint)FILEOP.FILEOP_SKIP;

			return (uint)FILEOP.FILEOP_DOIT;
		}

		protected virtual uint OnFileExtractComplete(IntPtr param1)
		{
			FILEPATHS filePaths = 
				(FILEPATHS)Marshal.PtrToStructure(param1, typeof(FILEPATHS));

			if (m_FileExtractComplete != null && filePaths.Win32Error == SetupApiWrapper.NO_ERROR)
				m_FileExtractComplete(filePaths.Target, System.EventArgs.Empty);

			return filePaths.Win32Error;
		}
		


		#region Envent Handler for collection
		
		protected virtual void OnListChanged(ListChangedEventArgs ev)
		{
			if (m_ListChanged != null)
				m_ListChanged(this, ev);
		}

		protected override void OnClear()
		{
			throw new NotSupportedException();
			//			base.OnClear ();
		}

		protected override void OnClearComplete()
		{
			OnListChanged(resetEvent);
		}

//		protected override void OnInsert(int index, object value)
//		{
//			throw new NotSupportedException();
////			base.OnInsert (index, value);
//		}

		protected override void OnInsertComplete(int index, object value)
		{
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
		}

		protected override void OnRemove(int index, object value)
		{
			throw new NotSupportedException();
			//			base.OnRemove (index, value);
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
		}

		protected override void OnSetComplete(int index, object oldValue, object newValue)
		{
			if(oldValue != newValue)
				OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
		}	
		#endregion

		#region IBindingList Implements
		
		bool IBindingList.AllowEdit
		{
			get {return false;}
		}

		bool IBindingList.AllowNew
		{
			get {return false;}
		}

		bool IBindingList.AllowRemove
		{
			get {return false;}
		}

		bool IBindingList.SupportsChangeNotification
		{
			get {return true;}
		}

		bool IBindingList.SupportsSearching
		{
			get {return false;}
		}

		bool IBindingList.SupportsSorting
		{
			get {return false;}
		}

		object IBindingList.AddNew()
		{
			throw new NotSupportedException();
			//			TFile file = new TFile();
			//			List.Add(file);
			//			return file;
		}


		bool IBindingList.IsSorted 
		{
			get { throw new NotSupportedException(); }
		}

		ListSortDirection IBindingList.SortDirection 
		{ 
			get { throw new NotSupportedException(); }
		}


		PropertyDescriptor IBindingList.SortProperty 
		{ 
			get { throw new NotSupportedException(); }
		}


		void IBindingList.AddIndex(PropertyDescriptor property)
		{
			throw new NotSupportedException();
		}

		void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction) 
		{
			throw new NotSupportedException(); 
		}

		int IBindingList.Find(PropertyDescriptor property, object key) 
		{
			throw new NotSupportedException(); 
		}

		void IBindingList.RemoveIndex(PropertyDescriptor property) 
		{
			throw new NotSupportedException(); 
		}

		void IBindingList.RemoveSort() 
		{
			throw new NotSupportedException(); 
		}

		#endregion


		private int m_ExtractFileIndex = -1;	// Target file index for extracting
		private bool m_ExtractAll  = false;		// if true, extract all

		public void ExtractAll()
		{
			m_ExtractAll = true;
			m_ExtractFileIndex = 0;
			
			PSP_FILE_CALLBACK callback = new PSP_FILE_CALLBACK(this.CallBack);
				
			uint setupIterateCabinetAction = (uint)SetupIterateCabinetAction.Extract;
			if (!SetupApiWrapper.SetupIterateCabinet(this.Name, 0, callback, setupIterateCabinetAction))
			{
                throw new Win32Exception((int)KernelApiWrapper.GetLastError());
			}	
		}

		public void Extract(string sFile)
		{
			int intI = 0;
			foreach(TFile file in this)
			{
				if (string.Compare(file.FullName, sFile, true)==0)
				{
					Extract(intI);
					return;
				}
				intI++;
			}
		}
	
		public void Extract(TFile file)
		{
			int intI = this.IndexOf(file);
			Extract(intI);
		}

		public void Extract(int index)
		{
			m_ExtractAll = false;
			m_ExtractFileIndex = index;
			
			PSP_FILE_CALLBACK callback = new PSP_FILE_CALLBACK(this.CallBack);
				
			uint setupIterateCabinetAction = (uint)SetupIterateCabinetAction.Extract;
			if (!SetupApiWrapper.SetupIterateCabinet(this.Name, 0, callback, setupIterateCabinetAction))
			{
                throw new Win32Exception((int)KernelApiWrapper.GetLastError());
			}	
		}

		private string MakeExtractFileName(TFile file)
		{
			string sFile = this.OutputDirectory;
			if (!this.IgnoreInsidePath && file.Path.Length >0)
			{
				sFile += System.IO.Path.DirectorySeparatorChar.ToString() + file.Path;
				if (!System.IO.Directory.Exists(sFile))
					System.IO.Directory.CreateDirectory(sFile);
			}
			sFile += System.IO.Path.DirectorySeparatorChar.ToString() + file.Name;
			sFile = sFile.Replace(@"\\", @"\");
			return sFile;
        }

        #region IDisposable

	    private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if(!this._disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if(disposing)
                {
                    // Dispose managed resources.
                    if (m_useTmpFile && File.Exists(this.m_Name)) File.Delete(m_Name);
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~TCabinetFile()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

	    public void Dispose()
	    {
	         Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
	    }

	    #endregion

    }
}
