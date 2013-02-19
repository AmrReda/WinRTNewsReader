#pragma once

#include "sqlite3.h"

using namespace Windows::Storage;
using namespace Windows::ApplicationModel;
using namespace Platform;

namespace SQLite 
{
	ref class Statement;
	public ref class Database sealed 
	{
		friend Statement;
	private:
		sqlite3* db;
		String^ _path;
	public:
		property bool Ready;
		property String^ Path
		{
			String^ Path::get() 
			{
				return _path;
			}
		}

		Database(Platform::String^ path)
		{
			_path = Package::Current->InstalledLocation->Path + "\\" + path;

			OutputDebugString(_path->Data());
			int rc = sqlite3_open16(_path->Data(), &db);
			Ready = rc == SQLITE_OK;
		}
		Database(Platform::String^ path, bool writablePath) 
		{
			if (writablePath)
			{
				_path = ApplicationData::Current->LocalFolder->Path + "\\" + path;
			}
			else
			{
				_path = Package::Current->InstalledLocation->Path + "\\" + path;
			}
			OutputDebugString(_path->Data());
			int rc = sqlite3_open16(_path->Data(), &db);
			Ready = rc == SQLITE_OK;
		}

		virtual ~Database() 
		{
			if (db != NULL) 
			{
				sqlite3_close(db);
				db = NULL;
			}
		}

		Statement^ PrepareStatement(Platform::String^ cmd);
	};
}