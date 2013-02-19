#include "pch.h"
#include "SQLiteStatement.h"
#include "SQLite.h"

using namespace SQLite;

Statement::Statement(Database^ database, Platform::String^ cmd) 
{
	int rc = sqlite3_prepare16_v2(database->db, cmd->Data(), -1, &stmt, 0);
	Valid = rc == SQLITE_OK;
}
