using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CarX;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClientSuspension
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class Main : BaseUnityPlugin
    { 
        public const string MODNAME = "C_Hydraulics", AUTHOR = "ValidUser", GUID = AUTHOR + "_" + MODNAME, VERSION = "1.0.0.0";
        internal readonly ManualLogSource log;
        internal readonly Harmony harmony;
        internal readonly Assembly assembly;
        public readonly string modFolder;
        public Main()
        {
            log = Logger;
            harmony = new Harmony(GUID);
            assembly = Assembly.GetExecutingAssembly();
            modFolder = Path.GetDirectoryName(assembly.Location); 
            InitConfig();
        }
        public void Start() { harmony.PatchAll(assembly); }
        public static Car LCCar;
        public static CARXCar LCXCar;
        public static Wheel FrontLeft;
        public static Wheel FrontRight;
        public static Wheel BackLeft;
        public static Wheel BackRight;
        public static RaceCar car;
        public static RaceCar[] allCars; 
        public static void CarSearch()
        {
            allCars = FindObjectsOfType<RaceCar>();
            for (int i = 0; i < allCars.Length; i++)
            {
                if (!allCars[i].isNetworkCar)
                {
                    car = allCars[i];
                    LCCar = allCars[i].GetComponentInParent<Car>();
                    LCXCar = allCars[i].GetComponentInParent<CARXCar>();
                    FrontLeft = LCXCar.GetWheel(WheelIndex.FrontLeft);
                    FrontRight = LCXCar.GetWheel(WheelIndex.FrontRight);
                    BackLeft = LCXCar.GetWheel(WheelIndex.RearLeft);
                    BackRight = LCXCar.GetWheel(WheelIndex.RearRight);
                    return;
                }
            }
        }  
        public static ConfigEntry<float> SpringStepFloat;  
        public static ConfigEntry<float> SpringJumpFloat;  
        public static ConfigEntry<float> SpringMaxFloat;  
        public static ConfigEntry<float> SpringMinFloat;  
        public static ConfigEntry<float> SpringJumpSpeedFloat;  
        public static ConfigEntry<KeyCode> keyCodeUP;
        public static ConfigEntry<KeyCode> keyCodeDOWN;
        public static ConfigEntry<KeyCode> keyCodeJUMP;
        public static ConfigEntry<bool> Front;
        public static ConfigEntry<bool> Back;
        public static ConfigEntry<bool> Together;
        public void InitConfig()
        {
            SpringStepFloat = Config.Bind("Suspension", "Spring Amount", 0.001f, new ConfigDescription("Amount to go up/down per Update() call", new AcceptableValueRange<float>(0.001f, 0.05f)));
            SpringJumpFloat = Config.Bind("Suspension", "Spring Jump", 0.25f, new ConfigDescription("Amount to jump by", new AcceptableValueRange<float>(0f, 10f)));
            SpringMaxFloat = Config.Bind("Suspension", "Spring Max", 3f, new ConfigDescription("Amount to extend springs MAX.", new AcceptableValueRange<float>(-4f, 10f)));
            SpringMinFloat = Config.Bind("Suspension", "Spring Min", 0.02f, new ConfigDescription("Amount to extend springs MIN.", new AcceptableValueRange<float>(-4f, 10f))); 
            SpringJumpSpeedFloat = Config.Bind("Suspension", "Spring Jump Speed", 0.1f, new ConfigDescription("Speed of *Jump*.", new AcceptableValueRange<float>(0.1f, 1f)));
            keyCodeUP = Config.Bind("Suspension", "UP Keybind", KeyCode.Q, "UP Key");
            keyCodeDOWN = Config.Bind("Suspension", "DOWN Keybind", KeyCode.E, "Down Key.");
            keyCodeJUMP = Config.Bind("Suspension", "JUMP Keybind", KeyCode.H, "Jump Key.");
            Front = Config.Bind("Suspension", "Front", true, "Use Front Suspension Together");
            Back = Config.Bind("Suspension", "Back", true, "Use Back Suspension Together");
            Together = Config.Bind("Suspension", "Together", true, "Use All Suspension Together"); 
        }
        IEnumerator UP()
        { 
            if (Together.Value == true)
            {
                if (LCCar == null || LCXCar == null) { CarSearch(); } 
                Front.Value = false;
                Back.Value = false;
                FrontLeft.maxSpringLen = SpringJumpFloat.Value;
                FrontRight.maxSpringLen = SpringJumpFloat.Value;
                BackLeft.maxSpringLen = SpringJumpFloat.Value;
                BackRight.maxSpringLen = SpringJumpFloat.Value;
            }
            else if (Front.Value == true)
            {
                if (LCCar == null || LCXCar == null) { CarSearch(); } 
                Together.Value = false;
                Back.Value = false;
                FrontLeft.maxSpringLen = SpringJumpFloat.Value;
                FrontRight.maxSpringLen = SpringJumpFloat.Value;
            }
            else if (Back.Value == true)
            {
                if (LCCar == null || LCXCar == null) { CarSearch(); } 
                Together.Value = false;
                Front.Value = false;
                BackLeft.maxSpringLen = SpringJumpFloat.Value;
                BackRight.maxSpringLen = SpringJumpFloat.Value;
            } 
            yield return new WaitForSeconds(SpringJumpSpeedFloat.Value);
            if (Together.Value == true)
            {
                if (LCCar == null || LCXCar == null) { CarSearch(); } 
                Front.Value = false;
                Back.Value = false;
                FrontLeft.maxSpringLen = SpringMinFloat.Value;
                FrontRight.maxSpringLen = SpringMinFloat.Value;
                BackLeft.maxSpringLen = SpringMinFloat.Value;
                BackRight.maxSpringLen = SpringMinFloat.Value;
            }
            else if (Front.Value == true)
            {
                if (LCCar == null || LCXCar == null) { CarSearch(); } 
                Together.Value = false;
                Back.Value = false;
                FrontLeft.maxSpringLen = SpringMinFloat.Value;
                FrontRight.maxSpringLen = SpringMinFloat.Value;
            }
            else if (Back.Value == true)
            {
                if (LCCar == null || LCXCar == null) { CarSearch(); } 
                Together.Value = false;
                Front.Value = false;
                BackLeft.maxSpringLen = SpringMinFloat.Value;
                BackRight.maxSpringLen = SpringMinFloat.Value;
            }  
        }
        
        void Update()
        { 
            if (SceneManager.GetSceneByName("SelectCar") == null) 
            {
                if (Input.GetKey(keyCodeUP.Value))
                {
                    if (Together.Value == true)
                    {
                        if (LCCar == null || LCXCar == null) { CarSearch(); }
                        Front.Value = false;
                        Back.Value = false;
                        FrontLeft.maxSpringLen -= SpringStepFloat.Value;
                        if (FrontLeft.maxSpringLen < SpringMinFloat.Value) { FrontLeft.maxSpringLen = SpringMinFloat.Value; }
                        FrontRight.maxSpringLen -= SpringStepFloat.Value;
                        if (FrontRight.maxSpringLen < SpringMinFloat.Value) { FrontRight.maxSpringLen = SpringMinFloat.Value; }
                        BackLeft.maxSpringLen -= SpringStepFloat.Value;
                        if (BackLeft.maxSpringLen < SpringMinFloat.Value) { BackLeft.maxSpringLen = SpringMinFloat.Value; }
                        BackRight.maxSpringLen -= SpringStepFloat.Value;
                        if (BackRight.maxSpringLen < SpringMinFloat.Value) { BackRight.maxSpringLen = SpringMinFloat.Value; }
                    }
                    else if (Front.Value == true)
                    {
                        if (LCCar == null || LCXCar == null) { CarSearch(); }
                        Together.Value = false;
                        Back.Value = false;
                        FrontLeft.maxSpringLen -= SpringStepFloat.Value;
                        if (FrontLeft.maxSpringLen < SpringMinFloat.Value) { FrontLeft.maxSpringLen = SpringMinFloat.Value; }
                        FrontRight.maxSpringLen -= SpringStepFloat.Value;
                        if (FrontRight.maxSpringLen < SpringMinFloat.Value) { FrontRight.maxSpringLen = SpringMinFloat.Value; }
                    }
                    else if (Back.Value == true)
                    {
                        if (LCCar == null || LCXCar == null) { CarSearch(); }
                        Together.Value = false;
                        Front.Value = false;
                        BackLeft.maxSpringLen -= SpringStepFloat.Value;
                        if (BackLeft.maxSpringLen < SpringMinFloat.Value) { BackLeft.maxSpringLen = SpringMinFloat.Value; }
                        BackRight.maxSpringLen -= SpringStepFloat.Value;
                        if (BackRight.maxSpringLen < SpringMinFloat.Value) { BackRight.maxSpringLen = SpringMinFloat.Value; }
                    }
                }
                if (Input.GetKey(keyCodeDOWN.Value))
                {
                    if (Together.Value == true)
                    {
                        Front.Value = false;
                        Back.Value = false;
                        if (LCCar == null || LCXCar == null) { CarSearch(); }
                        FrontLeft.maxSpringLen += SpringStepFloat.Value;
                        if (FrontLeft.maxSpringLen > SpringMaxFloat.Value) { FrontLeft.maxSpringLen = SpringMaxFloat.Value; }
                        FrontRight.maxSpringLen += SpringStepFloat.Value;
                        if (FrontRight.maxSpringLen > SpringMaxFloat.Value) { FrontRight.maxSpringLen = SpringMaxFloat.Value; }
                        BackLeft.maxSpringLen += SpringStepFloat.Value;
                        if (BackLeft.maxSpringLen > SpringMaxFloat.Value) { BackLeft.maxSpringLen = SpringMaxFloat.Value; }
                        BackRight.maxSpringLen += SpringStepFloat.Value;
                        if (BackRight.maxSpringLen > SpringMaxFloat.Value) { BackRight.maxSpringLen = SpringMaxFloat.Value; }
                    }
                    else if (Front.Value == true)
                    {
                        Together.Value = false;
                        Back.Value = false;
                        if (LCCar == null || LCXCar == null) { CarSearch(); }
                        FrontLeft.maxSpringLen += SpringStepFloat.Value;
                        if (FrontLeft.maxSpringLen > SpringMaxFloat.Value) { FrontLeft.maxSpringLen = SpringMaxFloat.Value; }
                        FrontRight.maxSpringLen += SpringStepFloat.Value;
                        if (FrontRight.maxSpringLen > SpringMaxFloat.Value) { FrontRight.maxSpringLen = SpringMaxFloat.Value; }
                    }
                    else if (Back.Value == true)
                    {
                        Together.Value = false;
                        Front.Value = false;
                        if (LCCar == null || LCXCar == null) { CarSearch(); }
                        BackLeft.maxSpringLen += SpringStepFloat.Value;
                        if (BackLeft.maxSpringLen > SpringMaxFloat.Value) { BackLeft.maxSpringLen = SpringMaxFloat.Value; }
                        BackRight.maxSpringLen += SpringStepFloat.Value;
                        if (BackRight.maxSpringLen > SpringMaxFloat.Value) { BackRight.maxSpringLen = SpringMaxFloat.Value; }
                    }
                }
                if (Input.GetKeyDown(keyCodeJUMP.Value)) { StartCoroutine(UP()); }
            } 
        }
    }
}