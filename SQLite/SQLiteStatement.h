#pragma once

#include "sqlite3.h"

using namespace std;

namespace SQLite 
{
	ref class Database; 
	public ref class Statement sealed {
	private:
		sqlite3_stmt* stmt;
	public:
		property bool Valid;
		Statement(Database^ database, Platform::String^ cmd);

		virtual ~Statement() 
		{
			sqlite3_finalize(stmt);
		}

		void Execute() 
		{
			int rc = sqlite3_step(stmt);
			if(rc != SQLITE_DONE)
			{
				throw Platform::Exception::CreateException(rc, "Failed to execute statement");
			}
		}

		bool HasMore() 
		{
			int rc = sqlite3_step(stmt);
			return rc == SQLITE_ROW;
		}

		Platform::String^ ColumnAsTextAt(int index) 
		{
			return ref new Platform::String((const wchar_t*) sqlite3_column_text16(stmt, index));
		}

		int ColumnAsIntAt(int index) 
		{
			return sqlite3_column_int(stmt, index);
		}

		double ColumnAsDoubleAt(int index) 
		{
			return sqlite3_column_double(stmt, index);
		}

		void BindText(int index, Platform::String^ param) 
		{
			int rc = sqlite3_bind_text16(stmt, index, param->Data(), -1, SQLITE_STATIC);
			if(rc != SQLITE_OK)
			{
				throw Platform::Exception::CreateException(rc, "Failed to bind to column");
			}
		}

		void BindInt(int index, int param) 
		{
			int rc = sqlite3_bind_int(stmt, index, param);
			if(rc != SQLITE_OK)
			{
				throw Platform::Exception::CreateException(rc, "Failed to bind to column");
			}
		}

		void BindDouble(int index, double param) 
		{
			int rc = sqlite3_bind_double(stmt, index, param);
			if(rc != SQLITE_OK)
			{
				throw Platform::Exception::CreateException(rc, "Failed to bind to column");
			}
		}
	};
}