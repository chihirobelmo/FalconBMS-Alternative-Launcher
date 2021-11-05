#ifndef _LOG_BOOK_H
#define _LOG_BOOK_H

#include <tchar.h>
#include "apisetcconv.h"
#include <stdio.h>

#define DllExport __declspec( dllexport )

extern "C"
{
    DllExport void CreateLbk(const char* fname, const char* callsign, const char* pilotname, const char* date);
}

typedef unsigned short ushort;

typedef struct
{
    ushort Flags;
    float FlightHours;

    //for mission complexity
    int WeaponsExpended;
    int ShotsAtPlayer;
    int AircraftInPackage;

    //mission score from Kevin
    int Score;

    //Air-to-Air
    int Kills;
    int HumanKills;
    int Killed;
    int KilledByHuman;
    int KilledBySelf;

    //Air-to-Ground
    int GroundUnitsKilled;
    int FeaturesDestroyed;
    int NavalUnitsKilled;

    //other
    int FriendlyFireKills;
    int WingmenLost;

} CAMP_MISS_STRUCT;

typedef unsigned char uchar;
typedef unsigned short WORD;

typedef struct _SYSTEMTIME {
    WORD wYear;
    WORD wMonth;
    WORD wDayOfWeek;
    WORD wDay;
    WORD wHour;
    WORD wMinute;
    WORD wSecond;
    WORD wMilliseconds;
} SYSTEMTIME, * PSYSTEMTIME, * LPSYSTEMTIME;

//define value identifying medals for array index
typedef enum
{
    AIR_FORCE_CROSS,
    SILVER_STAR,
    DIST_FLY_CROSS,
    AIR_MEDAL,
    KOREA_CAMPAIGN,
    LONGEVITY,
    NUM_MEDALS,
} LB_MEDAL;

//Ranks
typedef enum
{
    SEC_LT,
    LEIUTENANT,
    CAPTAIN,
    MAJOR,
    LT_COL,
    COLONEL,
    BRIG_GEN,
    NUM_RANKS,
} LB_RANK;

enum
{
    FILENAME_LEN = 32,
    PASSWORD_LEN = 10,
    PERSONAL_TEXT_LEN = 120,
    COMM_LEN = 12,
    _NAME_LEN_ = 20,
    _CALLSIGN_LEN_ = 12,
};

enum
{
    LB_INVALID_CALLSIGN = 0x01,
    LB_EDITABLE = 0x02,
    LB_OPPONENT = 0x04,
    LB_CHECKED = 0x08,
    LB_REFRESH_PILOT = 0x10,
    LB_LOADED_ONCE = 0x20,
};

enum
{
    NOPATCH = 70050,
    NOFACE = 60000,
    LOGBOOK_PICTURE_ID = 8649144,
    LOGBOOK_PICTURE_ID_2 = 8649145,
    LOGBOOK_SQUADRON_ID = 8649146,
    LOGBOOK_SQUADRON_ID_2 = 8649147,

    PATCHES_RESOURCE = 59998,
    PILOTS_RESOURCE = 59999,
};

typedef struct DogfightStats
{
    short MatchesWon;
    short MatchesLost;
    short MatchesWonVHum;
    short MatchesLostVHum;
    short Kills;
    short Killed;
    short HumanKills;
    short KilledByHuman;
} DF_STATS;

typedef struct CampaignStats
{
    short GamesWon;
    short GamesLost;
    short GamesTied;
    short Missions;
    long TotalScore;
    long TotalMissionScore;
    short ConsecMissions;
    short Kills;
    short Killed;
    short HumanKills;
    short KilledByHuman;
    short KilledBySelf;
    short AirToGround;
    short Static;
    short Naval;
    short FriendliesKilled;
    short MissSinceLastFriendlyKill;
} CAMP_STATS;

typedef struct Pilot
{
    _TCHAR Name[_NAME_LEN_ + 1];
    _TCHAR Callsign[_CALLSIGN_LEN_ + 1];
    _TCHAR Password[PASSWORD_LEN + 1];
    _TCHAR Commissioned[COMM_LEN + 1];
    _TCHAR OptionsFile[_CALLSIGN_LEN_ + 1];
    float FlightHours;
    float AceFactor;
    LB_RANK Rank;
    DF_STATS Dogfight;
    CAMP_STATS Campaign;
    uchar Medals[NUM_MEDALS];
    long PictureResource;
    _TCHAR Picture[FILENAME_LEN + 1];
    long PatchResource;
    _TCHAR Patch[FILENAME_LEN + 1];
    _TCHAR Personal[PERSONAL_TEXT_LEN + 1];
    _TCHAR Squadron[_NAME_LEN_];
    short voice; // index from 0 - 11 indicating which voice they want
    long CheckSum; // If this value is ever NON zero after Decrypting, the Data has been modified
} LB_PILOT;

class LogBookData
{
private:
    void EncryptPwd(void);
public:
    LB_PILOT Pilot;

    LogBookData(const char* callsign, const char* pilotname, const char* date);
    void Initialize(const char* callsign, const char* pilotname, const char* date);
    void SaveData(const char* fname, const char* callsign);

    CAMP_STATS* GetCampaign(void)
    {
        return &Pilot.Campaign;
    }
    DF_STATS* GetDogfight(void)
    {
        return &Pilot.Dogfight;
    }
    LB_PILOT* GetPilot(void)
    {
        return &Pilot;
    }
    // This is used for remote pilots...so I can get them in the class used for drawing the UI
    void SetPilot(LB_PILOT* data)
    {
        if (data) memcpy(&Pilot, data, sizeof(Pilot));
    }

    void SetFlightHours(float Hours)
    {
        Pilot.FlightHours = Hours;
    }
    void UpdateFlightHours(float Hours)
    {
        Pilot.FlightHours += Hours;
    }

    _TCHAR* GetPicture(void)
    {
        return Pilot.Picture;
    }
    long GetPictureResource(void)
    {
        return Pilot.PictureResource;
    }
    void SetPicture(_TCHAR* filename)
    {
        if (_tcslen(filename) <= FILENAME_LEN) wcscpy_s(Pilot.Picture, filename);

        Pilot.PictureResource = 0;
    }

    _TCHAR* GetPatch(void)
    {
        return Pilot.Patch;
    }
    long GetPatchResource(void)
    {
        return Pilot.PatchResource;
    }
    void SetPatch(_TCHAR* filename)
    {
        if (_tcslen(filename) <= FILENAME_LEN) wcscpy_s(Pilot.Patch, filename);

        Pilot.PatchResource = 0;
    }

    _TCHAR* Name(void)
    {
        return Pilot.Name;
    }
    void SetName(_TCHAR* Name)
    {
        if (_tcslen(Name) <= _NAME_LEN_) wcscpy_s(Pilot.Name, Name);
    }

    _TCHAR* Callsign(void)
    {
        return Pilot.Callsign;
    }
    void SetCallsign(_TCHAR* Callsign)
    {
        if (_tcslen(Callsign) <= _CALLSIGN_LEN_) wcscpy_s(Pilot.Callsign, Callsign);
    }

    _TCHAR* Squadron(void)
    {
        return Pilot.Squadron;
    }
    void SetSquadron(_TCHAR* Squadron)
    {
        if (_tcslen(Squadron) <= _NAME_LEN_) wcscpy_s(Pilot.Squadron, Squadron);
    }

    _TCHAR* Personal(void)
    {
        return Pilot.Personal;
    }
    void SetPersonal(_TCHAR* Personal)
    {
        if (_tcslen(Personal) <= PERSONAL_TEXT_LEN) wcscpy_s(Pilot.Personal, Personal);
    }

    _TCHAR* OptionsFile(void)
    {
        return Pilot.OptionsFile;
    }
    void SetOptionsFile(_TCHAR* OptionsFile)
    {
        if (_tcslen(OptionsFile) <= _CALLSIGN_LEN_) wcscpy_s(Pilot.OptionsFile, OptionsFile);
    }

    float AceFactor(void)
    {
        return Pilot.AceFactor;
    }
    void SetAceFactor(float Factor)
    {
        Pilot.AceFactor = Factor;
    }

    _TCHAR* Commissioned(void)
    {
        return Pilot.Commissioned;
    }
    void SetCommissioned(_TCHAR* Date)
    {
        if (_tcslen(Date) <= COMM_LEN) wcscpy_s(Pilot.Commissioned, Date);
    }
    float FlightHours(void)
    {
        return Pilot.FlightHours;
    }

    void SetVoice(short newvoice)
    {
        Pilot.voice = newvoice;
    }
    uchar Voice(void)
    {
        return (uchar)Pilot.voice;
    }
};

extern class LogBookData LogBook;
extern class LogBookData UI_logbk;
#endif