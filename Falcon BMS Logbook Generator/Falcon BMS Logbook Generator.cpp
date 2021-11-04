#include <cISO646>
#include <tchar.h>
#include <time.h>
#include "Falcon BMS Logbook Generator.h"
#include <string.h>

#pragma warning(disable : 4244)  // for all the short += short's

class LogBookData LogBook;

#define _USE_REGISTRY_ 1
#define BAD_READ 2

int MissionResult = 0;
extern int LogState;
extern bool g_bDisableCrashEjectCourtMartials; // JB 010118

void EncryptBuffer(uchar startkey, uchar* buffer, long length);

char FalconDataDirectory[260];

void Test()
{
    LogBookData UI_logbk;
    UI_logbk.SaveData();
}

LogBookData::LogBookData(void)
{
    Initialize();
}

void LogBookData::Initialize(void)
{
    wcscpy_s(Pilot.Name, _T("Joe Pilot"));
    wcscpy_s(Pilot.Callsign, _T("Viper"));
    wcscpy_s(Pilot.OptionsFile, _T("Default"));
    wcscpy_s(Pilot.Password, _T(""));
    EncryptPwd();
    Pilot.Rank = SEC_LT;
    Pilot.AceFactor = 1.0f;
    Pilot.FlightHours = 0.0F;
    memset(&Pilot.Campaign, 0, sizeof(CAMP_STATS));
    memset(&Pilot.Dogfight, 0, sizeof(DF_STATS));
    memset(Pilot.Medals, 0, sizeof(uchar) * NUM_MEDALS);
    Pilot.Picture[0] = 0;
    Pilot.PictureResource = NOFACE;
    Pilot.Patch[0] = 0;
    Pilot.PatchResource = NOPATCH;
    Pilot.Personal[0] = 0;
    Pilot.Squadron[0] = 0;
    Pilot.voice = 0;

    Pilot.CheckSum = 0;
}

void LogBookData::SaveData(void)
{
    FILE* fp;
    _TCHAR path[260];

    swprintf_s(path, _T("Viper.lbk"));

    char* fname{};
    char* mode{};
    int flag{};

    fp = _fsopen(fname, mode, flag);

    EncryptBuffer(0x58, (uchar*)&Pilot, sizeof(LB_PILOT));

    fwrite(&Pilot, sizeof(LB_PILOT), 1, fp);
    fclose(fp);
}

char MasterXOR[] = "FreeFalcon is your Master";

void EncryptBuffer(uchar startkey, uchar* buffer, long length)
{
    long i, xrlen, idx;
    uchar* ptr;
    uchar nextkey;

    if (not buffer or length <= 0)
        return;

    idx = 0;
    xrlen = strlen(MasterXOR);

    ptr = buffer;

    for (i = 0; i < length; i++)
    {
        *ptr xor_eq MasterXOR[(idx++) % xrlen];
        *ptr xor_eq startkey;
        nextkey = *ptr++;
        startkey = nextkey;
    }
}

static char PwdMask[] = "Who needs a password";
static char PwdMask2[] = "Repent, FreeFalcon is coming";

void LogBookData::EncryptPwd(void)
{
    int i;
    char* ptr;

    ptr = (char*)Pilot.Password;

    for (i = 0; i < PASSWORD_LEN; i++)
    {
        *ptr xor_eq PwdMask[i % strlen(PwdMask)];
        *ptr xor_eq PwdMask2[i % strlen(PwdMask2)];
        ptr++;
    }
}
