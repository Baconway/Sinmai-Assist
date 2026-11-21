using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MAI2.Util;
using Manager;
using Manager.UserDatas;
using MelonLoader;
using SinmaiAssist.Utils;
using UnityEngine;

namespace SinmaiAssist.GUI;

public class UserDataPanel
{
    private static UserData _player1 = null;
    private static UserData _player2 = null;
    private static bool _isNewItem = false;
    
    private enum CollectionType
    {
        Icon = UserData.Collection.Icon,
        Plate = UserData.Collection.Plate,
        Title = UserData.Collection.Title,
        Partner = UserData.Collection.Partner,
        Frame = UserData.Collection.Frame
    }
    
    private static string[] _userInputId = ["", "", "", "", "", "", ""];
    private static string _charaInputId = "";
    private static string _charaInputLevel = "1";
    
    public static void OnGUI()
    {
        GUILayout.Label($"User Info", MainGUI.Style.Title);
        try
        {
            _player1 = Singleton<UserDataManager>.Instance.GetUserData(0);
            _player2 = Singleton<UserDataManager>.Instance.GetUserData(1);
        }
        catch (Exception e)
        {
            // ignore
        }
        GUILayout.Label($"1P: {_player1.Detail.UserName} ({_player1.Detail.UserID})", MainGUI.Style.Text);
        GUILayout.Label($"2P: {_player2.Detail.UserName} ({_player2.Detail.UserID})", MainGUI.Style.Text);
        
        GUILayout.Label("Add Collections", MainGUI.Style.Title);
        foreach (CollectionType type in Enum.GetValues(typeof(CollectionType)))
        {
            GUILayout.BeginHorizontal();
            int typeId = (int)type;
            GUILayout.Label(type.ToString(), new GUIStyle(MainGUI.Style.Text){fixedWidth = 50});
            _userInputId[typeId] = GUILayout.TextField(_userInputId[typeId]);
            if (GUILayout.Button("Add", new GUIStyle(MainGUI.Style.Button){ fixedWidth = 50}))
            {
                AddCollections(0, type, _userInputId[typeId]);
                AddCollections(1, type, _userInputId[typeId]);
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Label("Remove Collections", MainGUI.Style.Title);
        foreach (CollectionType type in Enum.GetValues(typeof(CollectionType)))
        {
            GUILayout.BeginHorizontal();
            int typeId = (int)type;
            GUILayout.Label(type.ToString(), new GUIStyle(MainGUI.Style.Text) { fixedWidth = 50 });
            _userInputId[typeId] = GUILayout.TextField(_userInputId[typeId]);
            if (GUILayout.Button("Remove", new GUIStyle(MainGUI.Style.Button) { fixedWidth = 50 }))
            {
                RemoveCollections(0, type, _userInputId[typeId]);
                RemoveCollections(1, type, _userInputId[typeId]);
            }
            GUILayout.EndHorizontal();
        }

        _isNewItem = GUILayout.Toggle(_isNewItem, "Is New Item");

        GUILayout.Label("Tour Members (Characters)", MainGUI.Style.Title);
        GUILayout.BeginHorizontal();
        GUILayout.Label("ID", new GUIStyle(MainGUI.Style.Text) { fixedWidth = 25 });
        _charaInputId = GUILayout.TextField(_charaInputId, GUILayout.Width(60));
        GUILayout.Label("Lvl", new GUIStyle(MainGUI.Style.Text) { fixedWidth = 25 });
        _charaInputLevel = GUILayout.TextField(_charaInputLevel, GUILayout.Width(60));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add", new GUIStyle(MainGUI.Style.Button) { fixedWidth = 60 }))
        {
            AddCharacterUI(0, _charaInputId);
            AddCharacterUI(1, _charaInputId);
        }
        if (GUILayout.Button("Remove", new GUIStyle(MainGUI.Style.Button) { fixedWidth = 60 }))
        {
            RemoveCharacterUI(0, _charaInputId);
            RemoveCharacterUI(1, _charaInputId);
        }
        if (GUILayout.Button("Set Level", new GUIStyle(MainGUI.Style.Button) { fixedWidth = 70 }))
        {
            SetCharacterLevelUI(0, _charaInputId, _charaInputLevel);
            SetCharacterLevelUI(1, _charaInputId, _charaInputLevel);
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Label("Unlock Music", MainGUI.Style.Title);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Music", new GUIStyle(MainGUI.Style.Text){fixedWidth = 50});
        _userInputId[0] = GUILayout.TextField(_userInputId[0]);
        if (GUILayout.Button("Add", new GUIStyle(MainGUI.Style.Button){ fixedWidth = 50}))
        {
            UnlockMusic(0, _userInputId[0]);
            UnlockMusic(1, _userInputId[0]);
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Label("MaiMile", MainGUI.Style.Title);
        GUILayout.BeginHorizontal();
        GUILayout.Label("MaiMile", new GUIStyle(MainGUI.Style.Text){fixedWidth = 50});
        _userInputId[6] = GUILayout.TextField(_userInputId[6]);
        if (GUILayout.Button("Add", new GUIStyle(MainGUI.Style.Button){ fixedWidth = 50}))
        {
            AddMaiMile(0, _userInputId[6]);
            AddMaiMile(1, _userInputId[6]);
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Label("User Data Backup", MainGUI.Style.Title);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("1P", MainGUI.Style.Button)) User.ExportBackupData(0);
        if (GUILayout.Button("2P", MainGUI.Style.Button)) User.ExportBackupData(1);
        GUILayout.EndHorizontal();
        
    }

    private static void AddCollections(long index, CollectionType type, string input)
    {
        UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
        if (userData.IsGuest())
        {
            GameMessageManager.SendMessage((int)index,"Guest Account\nUnable to add collections");
            return;
        }
        try
        {
            if (int.TryParse(input, out int id))
            {
                if (userData.AddCollections((UserData.Collection)type, id, _isNewItem))
                {
                    GameMessageManager.SendMessage((int)index,$"Add Collections \n{type} {id}" + (_isNewItem ? " (New Item)" : "") );
                }
                else
                {
                    GameMessageManager.SendMessage((int)index,$"Failed to add Collections or already added\n{type} {id}");
                }
            }
            else
            {
                GameMessageManager.SendMessage((int)index,$"Invalid ID\n {input}");
            }
        }
        catch (Exception e)
        {
            GameMessageManager.SendMessage((int)index,$"Unknown error");
            MelonLogger.Error(e);
        }
    }

    private const int DefaultCollectionId = 1;
    private const int RandomCollectionId = 2;
    private const int FavoriteRandomCollectionId = 3;
    private const int ExcludedRandomIconId = 10;

    private static void RemoveCollections(long index, CollectionType type, string input)
    {
        UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
        if (userData == null || userData.IsGuest())
        {
            GameMessageManager.SendMessage((int)index, "Guest Account\nUnable to remove collections");
            return;
        }
        try
        {
            if (int.TryParse(input, out int id))
            {
                if (RemoveCollections(userData, (UserData.Collection)type, id))
                {
                    GameMessageManager.SendMessage((int)index, $"Remove Collections \n{type} {id}");
                }
                else
                {
                    GameMessageManager.SendMessage((int)index, $"Failed to remove Collections or not owned\n{type} {id}");
                }
            }
            else
            {
                GameMessageManager.SendMessage((int)index, $"Invalid ID\n {input}");
            }
        }
        catch (Exception e)
        {
            GameMessageManager.SendMessage((int)index, "Unknown error");
            MelonLogger.Error(e);
        }
    }

    internal static bool RemoveCollections(UserData userData, UserData.Collection type, int id)
    {
        if (userData == null || userData.IsGuest())
        {
            return false;
        }

        if (!TryGetCollectionContext(
                userData,
                type,
                out List<UserItem> itemList,
                out List<int> newList,
                out List<int> favoriteList,
                out Action<int> resetState)
            || itemList == null)
        {
            return false;
        }

        bool removed = false;
        foreach (UserItem item in itemList)
        {
            if (item != null && item.itemId == id && item.stock > 0)
            {
                item.stock = 0;
                removed = true;
            }
        }

        if (!removed)
        {
            return false;
        }

        newList?.RemoveAll(x => x == id);
        favoriteList?.RemoveAll(x => x == id);
        resetState?.Invoke(id);
        return true;
    }

    private static bool TryGetCollectionContext(
        UserData userData,
        UserData.Collection type,
        out List<UserItem> itemList,
        out List<int> newList,
        out List<int> favoriteList,
        out Action<int> resetState)
    {
        switch (type)
        {
            case UserData.Collection.Icon:
                itemList = userData.IconList;
                newList = userData.NewIconList;
                favoriteList = userData.FavoriteIconList;
                resetState = id => ResetEquippedAndViewed(
                    id,
                    () => userData.Detail.EquipIconID,
                    val => userData.Detail.EquipIconID = val,
                    () => userData.Detail.ViewIconID,
                    val => userData.Detail.ViewIconID = val,
                    userData.FavoriteIconList,
                    userData.IconList,
                    isIcon: true);
                return true;

            case UserData.Collection.Plate:
                itemList = userData.PlateList;
                newList = userData.NewPlateList;
                favoriteList = userData.FavoritePlateList;
                resetState = id => ResetEquippedAndViewed(
                    id,
                    () => userData.Detail.EquipPlateID,
                    val => userData.Detail.EquipPlateID = val,
                    () => userData.Detail.ViewPlateID,
                    val => userData.Detail.ViewPlateID = val,
                    userData.FavoritePlateList,
                    userData.PlateList,
                    isIcon: false);
                return true;

            case UserData.Collection.Title:
                itemList = userData.TitleList;
                newList = userData.NewTitleList;
                favoriteList = userData.FavoriteTitleList;
                resetState = id => ResetEquippedAndViewed(
                    id,
                    () => userData.Detail.EquipTitleID,
                    val => userData.Detail.EquipTitleID = val,
                    () => userData.Detail.ViewTitleID,
                    val => userData.Detail.ViewTitleID = val,
                    userData.FavoriteTitleList,
                    userData.TitleList,
                    isIcon: false);
                return true;

            case UserData.Collection.Partner:
                itemList = userData.PartnerList;
                newList = userData.NewPartnerList;
                favoriteList = null;
                resetState = id =>
                {
                    if (userData.Detail.EquipPartnerID == id)
                    {
                        userData.Detail.EquipPartnerID = DefaultCollectionId;
                    }
                };
                return true;

            case UserData.Collection.Frame:
                itemList = userData.FrameList;
                newList = userData.NewFrameList;
                favoriteList = userData.FavoriteFrameList;
                resetState = id => ResetEquippedAndViewed(
                    id,
                    () => userData.Detail.EquipFrameID,
                    val => userData.Detail.EquipFrameID = val,
                    () => userData.Detail.ViewFrameID,
                    val => userData.Detail.ViewFrameID = val,
                    userData.FavoriteFrameList,
                    userData.FrameList,
                    isIcon: false);
                return true;

            default:
                itemList = null;
                newList = null;
                favoriteList = null;
                resetState = null;
                return false;
        }
    }

    private static void ResetEquippedAndViewed(
        int removedId,
        Func<int> getEquip,
        Action<int> setEquip,
        Func<int> getView,
        Action<int> setView,
        List<int> favorites,
        List<UserItem> items,
        bool isIcon)
    {
        int currentEquip = getEquip();

        if (currentEquip == removedId)
        {
            setEquip(DefaultCollectionId);
            return;
        }

        if (currentEquip == FavoriteRandomCollectionId)
        {
            if (favorites == null || favorites.Count == 0)
            {
                setEquip(DefaultCollectionId);
            }
            else if (getView() == removedId)
            {
                setView(favorites[0]);
            }
            return;
        }

        if (currentEquip == RandomCollectionId && getView() == removedId)
        {
            int fallbackId = DefaultCollectionId;
            if (items != null)
            {
                foreach (UserItem item in items)
                {
                    if (item != null && item.stock > 0 && item.itemId != removedId && (!isIcon || item.itemId != ExcludedRandomIconId))
                    {
                        fallbackId = item.itemId;
                        break;
                    }
                }
            }
            setView(fallbackId);
        }
    }

    private static void AddCharacterUI(long index, string inputId)
    {
        UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
        if (userData == null || userData.IsGuest())
        {
            GameMessageManager.SendMessage((int)index, "Guest Account\nUnable to add character");
            return;
        }
        try
        {
            if (int.TryParse(inputId, out int id))
            {
                if (AddCharacter(userData, id, _isNewItem))
                {
                    uint lvl = 1;
                    if (uint.TryParse(_charaInputLevel, out lvl) && lvl > 1)
                    {
                        SetCharacterLevel(userData, id, lvl);
                    }
                    GameMessageManager.SendMessage((int)index, $"Add Character\nID: {id}" + (_isNewItem ? " (New)" : "") + (lvl > 1 ? $"\nLvl: {lvl}" : ""));
                }
                else
                {
                    GameMessageManager.SendMessage((int)index, $"Character {id} is already added.\nUse 'Set Level' to change level.");
                }
            }
            else
            {
                GameMessageManager.SendMessage((int)index, $"Invalid Character ID\n{inputId}");
            }
        }
        catch (Exception e)
        {
            GameMessageManager.SendMessage((int)index, "Unknown error");
            MelonLogger.Error(e);
        }
    }

    private static void RemoveCharacterUI(long index, string inputId)
    {
        UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
        if (userData == null || userData.IsGuest())
        {
            GameMessageManager.SendMessage((int)index, "Guest Account\nUnable to remove character");
            return;
        }
        try
        {
            if (int.TryParse(inputId, out int id))
            {
                if (RemoveCharacter(userData, id))
                {
                    GameMessageManager.SendMessage((int)index, $"Remove Character\nID: {id}");
                }
                else
                {
                    GameMessageManager.SendMessage((int)index, $"Failed to remove character or not owned\nID: {id}");
                }
            }
            else
            {
                GameMessageManager.SendMessage((int)index, $"Invalid Character ID\n{inputId}");
            }
        }
        catch (Exception e)
        {
            GameMessageManager.SendMessage((int)index, "Unknown error");
            MelonLogger.Error(e);
        }
    }

    private static void SetCharacterLevelUI(long index, string inputId, string inputLevel)
    {
        UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
        if (userData == null || userData.IsGuest())
        {
            GameMessageManager.SendMessage((int)index, "Guest Account\nUnable to modify character");
            return;
        }
        try
        {
            if (!int.TryParse(inputId, out int id))
            {
                GameMessageManager.SendMessage((int)index, $"Invalid Character ID\n{inputId}");
                return;
            }

            if (!uint.TryParse(inputLevel, out uint level))
            {
                level = 1;
            }

            if (SetCharacterLevel(userData, id, level))
            {
                var chara = userData.CharaList.FirstOrDefault(c => c.ID == id);
                GameMessageManager.SendMessage((int)index, $"Set Character Level\nID: {id}\nLvl: {chara?.Level} (Stars: {chara?.Awakening})");
            }
            else
            {
                GameMessageManager.SendMessage((int)index, $"Failed to set character level.\nCharacter {id} is not owned. Please add first.");
            }
        }
        catch (Exception e)
        {
            GameMessageManager.SendMessage((int)index, "Unknown error");
            MelonLogger.Error(e);
        }
    }

    public static bool AddCharacter(UserData userData, int charaId, bool isNew = false)
    {
        if (userData == null || userData.IsGuest())
        {
            return false;
        }

        if (userData.CharaList.Any(c => c.ID == charaId))
        {
            return false;
        }

        UserChara userChara = new UserChara(charaId);
        userData.CharaList.Add(userChara);

        if (isNew && !userData.NewCharaList.Contains(charaId))
        {
            userData.NewCharaList.Add(charaId);
        }

        try
        {
            userChara.CalcLevelToAwake();
        }
        catch
        {
        }

        try
        {
            userData.UpdateTotalAwake();
        }
        catch
        {
        }

        if (userData.Detail != null)
        {
            userData.Detail.TotalAwake = (int)userData.CharaList.Sum(c => (long)c.Awakening);
        }

        return true;
    }

    public static bool RemoveCharacter(UserData userData, int charaId)
    {
        if (userData == null || userData.IsGuest())
        {
            return false;
        }

        if (!userData.CharaList.Any(c => c.ID == charaId))
        {
            return false;
        }

        // Remove ALL occurrences of charaId to heal any duplicate entries
        userData.CharaList.RemoveAll(c => c.ID == charaId);
        userData.NewCharaList.RemoveAll(x => x == charaId);
        userData.FavoriteCharaList.RemoveAll(x => x == charaId);

        if (userData.Detail != null)
        {
            int fallbackId = 0;
            if (userData.CharaList.Any(c => c.ID == 101))
            {
                fallbackId = 101;
            }
            else if (userData.CharaList.Count > 0)
            {
                fallbackId = userData.CharaList[0].ID;
            }

            if (userData.Detail.CharaSlot != null)
            {
                for (int i = 0; i < userData.Detail.CharaSlot.Length; i++)
                {
                    if (userData.Detail.CharaSlot[i] == charaId)
                    {
                        userData.Detail.CharaSlot[i] = fallbackId;
                    }
                }
            }

            if (userData.Detail.CharaLockSlot != null)
            {
                for (int i = 0; i < userData.Detail.CharaLockSlot.Length; i++)
                {
                    if (userData.Detail.CharaLockSlot[i] == charaId)
                    {
                        userData.Detail.CharaLockSlot[i] = 0;
                    }
                }
            }

            userData.Detail.TotalAwake = (int)userData.CharaList.Sum(c => (long)c.Awakening);
        }

        try
        {
            userData.UpdateTotalAwake();
        }
        catch
        {
        }

        return true;
    }

    public static bool SetCharacterLevel(UserData userData, int charaId, uint level)
    {
        if (userData == null || userData.IsGuest())
        {
            return false;
        }

        var matching = userData.CharaList.Where(c => c.ID == charaId).ToList();
        if (matching.Count == 0)
        {
            return false;
        }

        // Heal/deduplicate if multiple instances exist
        if (matching.Count > 1)
        {
            for (int i = 1; i < matching.Count; i++)
            {
                userData.CharaList.Remove(matching[i]);
            }
        }

        var chara = matching[0];

        if (level == 0)
        {
            level = 1;
        }
        else if (level > 999999)
        {
            level = 999999;
        }

        chara.Level = level;
        chara.DisplayLevel = chara.ClacDisplayLevel();
        chara.Reincarnation = chara.ClacReincarnationLevel();

        try
        {
            chara.CalcLevelToAwake();
        }
        catch
        {
        }

        if (chara.Awakening == 0 && level >= 10)
        {
            if (level >= 10000) chara.Awakening = 6;
            else if (level >= 1000) chara.Awakening = 5;
            else if (level >= 300) chara.Awakening = 4;
            else if (level >= 100) chara.Awakening = 3;
            else if (level >= 50) chara.Awakening = 2;
            else chara.Awakening = 1;
        }

        try
        {
            userData.UpdateTotalAwake();
        }
        catch
        {
        }

        if (userData.Detail != null)
        {
            userData.Detail.TotalAwake = (int)userData.CharaList.Sum(c => (long)c.Awakening);
        }

        return true;
    }

    public static bool SetCharacterLevel(UserData userData, int charaId, uint level, uint reincarnation)
    {
        return SetCharacterLevel(userData, charaId, reincarnation * 10000 + level);
    }

    private static void UnlockMusic(long index, string input)
    {
        UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
        if (userData.IsGuest())
        {
            GameMessageManager.SendMessage((int)index,"Guest Account\nUnable to unlock music");
            return;
        }
        try
        {
            if (int.TryParse(input, out int id))
            {
                if (!userData.IsUnlockMusic(UserData.MusicUnlock.Base, id))
                {
                    if (userData.AddUnlockMusic(UserData.MusicUnlock.Base, id))
                    {
                        GameMessageManager.SendMessage((int)index,$"Unlock Music \n{id}");
                    }
                    else
                    {
                        GameMessageManager.SendMessage((int)index,$"Failed to unlock music or already unlocked \n{id}");
                    }
                }
                else if(!userData.IsUnlockMusic(UserData.MusicUnlock.Master, id))
                {
                    userData.AddUnlockMusic(UserData.MusicUnlock.Master, id);
                    userData.AddUnlockMusic(UserData.MusicUnlock.ReMaster, id);
                    GameMessageManager.SendMessage((int)index,$"Unlock Master \n{id}");
                }
                else
                {
                    GameMessageManager.SendMessage((int)index,$"Failed to unlock Master or already unlocked\n{id}");
                }
            }
            else
            {
                GameMessageManager.SendMessage((int)index,$"Invalid ID\n {input}");
            }
        }
        catch (Exception e)
        {
            GameMessageManager.SendMessage((int)index,$"Unknown error");
            MelonLogger.Error(e);
        }
    }

    private static void AddMaiMile(long index, string input)
    {
        UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
        if (SinmaiAssist.GameVersion < 25000)
        {
            GameMessageManager.SendMessage((int)index,"MaiMile is not supported in this version");
            return;
        }
        if (userData.IsGuest())
        {
            GameMessageManager.SendMessage((int)index,"Guest Account\nUnable to add MaiMile");
            return;
        }
        try
        {
            if (int.TryParse(input , out int addMile))
            {
                var haveMile = userData.Detail.Point;
                if (haveMile + addMile >= 99999)
                    addMile = 99999 - haveMile;
                var addMileBefore = haveMile + addMile;
                
                userData.AddPresentMile(addMile);
                GameMessageManager.SendMessage((int)index,$"Add {addMile} MaiMile\n ({haveMile} -> {addMileBefore})");
            }
            else
            {
                GameMessageManager.SendMessage((int)index,$"Invalid MaiMile\n {input}");
            }
        }
        catch (Exception e)
        {
            GameMessageManager.SendMessage((int)index,$"Unknown error");
            MelonLogger.Error(e);
        }
    }
}
