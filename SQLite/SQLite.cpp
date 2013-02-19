#include "pch.h"
#include "SQLite.h"
#include "SQLiteStatement.h"

using namespace SQLite;

Statement^ Database::PrepareStatement(Platform::String^ cmd) {
	return ref new Statement(this, cmd);
}

