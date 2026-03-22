using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Gtr2MemOpsTool.Models
{
    public class GridDataMemoryRegion
    {
        public MemoryRegion Region { get; set; }
        public int OffsetStatic { get; set; } // Offset from some base address. Would be better to detect this dynamically.
        public GridDataMemoryRegion(MemoryRegion region, int offsetStatic)
        {
            Region = region;
            OffsetStatic = offsetStatic;
            MemoryField[] Fields =
            [
                new MemoryField("MemAddrRef_A", typeof(Int32), 1, 0x00), // -20692 rcd_Script
                new MemoryField("slot_id", typeof(Int32), 1, 0x00),
                new MemoryField("pitgroup_id", typeof(Int32), 1, 0x00),
                new MemoryField("x_UnknVar_a", typeof(Int32), 1, 0x00),
                new MemoryField("MemAddrRef_B", typeof(Int32), 1, 0x00),
                new MemoryField("x_Unkn_Flag_A", typeof(Int32), 1, 0x00),
                new MemoryField("MemAddrRef_C", typeof(Int32), 1, 0x00),
                new MemoryField("x_Unkn_Flag_B", typeof(Int32), 1, 0x00),
                //new MemoryField("Coords_A", typeof(float), 3, 0x00),
                new MemoryField("Coords_A_1", typeof(float), 1, 0x00),
                new MemoryField("Coords_A_2", typeof(float), 1, 0x00),
                new MemoryField("Coords_A_3", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_aa", typeof(byte), 140, 0x00), // 44-184
                new MemoryField("InputThrottle_A", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_ab", typeof(byte), 2604, 0x00), // 188-2792
                //new MemoryField("Coords_B", typeof(float), 3, 0x00),
                new MemoryField("Coords_B_1", typeof(float), 1, 0x00),
                new MemoryField("Coords_B_2", typeof(float), 1, 0x00),
                new MemoryField("Coords_B_3", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_ac", typeof(byte), 2452, 0x00), // 2804-5256
                new MemoryField("x_UnknVar_ab", typeof(float), 1, 0x00),
                new MemoryField("InputSteering_A", typeof(float), 64, 0x00),
                new MemoryField("x_UnknVar_ac", typeof(Int32), 1, 0x00),
                new MemoryField("x_UnknVar_ad", typeof(Int32), 1, 0x00),
                //new MemoryField("car_CameraTarget", typeof(float), 3, 0x00),
                new MemoryField("car_CameraTarget_1", typeof(float), 1, 0x00),
                new MemoryField("car_CameraTarget_2", typeof(float), 1, 0x00),
                new MemoryField("car_CameraTarget_3", typeof(float), 1, 0x00),
                //new MemoryField("car_Eyepoint", typeof(float), 3, 0x00),
                new MemoryField("car_Eyepoint_1", typeof(float), 1, 0x00),
                new MemoryField("car_Eyepoint_2", typeof(float), 1, 0x00),
                new MemoryField("car_Eyepoint_3", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_b", typeof(byte), 24, 0x00), // 5548-5572
                //new MemoryField("car_MirrorPos", typeof(float), 3, 0x00),
                new MemoryField("car_MirrorPos_1", typeof(float), 1, 0x00),
                new MemoryField("car_MirrorPos_2", typeof(float), 1, 0x00),
                new MemoryField("car_MirrorPos_3", typeof(float), 1, 0x00),
                //new MemoryField("x_UnknVar_mirror_A", typeof(float), 3, 0x00), // 5584-5596
                new MemoryField("x_UnknVar_mirror_A_1", typeof(float), 1, 0x00),
                new MemoryField("x_UnknVar_mirror_A_2", typeof(float), 1, 0x00),
                new MemoryField("x_UnknVar_mirror_A_3", typeof(float), 1, 0x00),
                //new MemoryField("x_UnknVar_mirror_B", typeof(float), 3, 0x00),
                new MemoryField("x_UnknVar_mirror_B_1", typeof(float), 1, 0x00),
                new MemoryField("x_UnknVar_mirror_B_2", typeof(float), 1, 0x00),
                new MemoryField("x_UnknVar_mirror_B_3", typeof(float), 1, 0x00),
                //new MemoryField("x_UnknVar_mirror_C", typeof(float), 3, 0x00), // 5608-5620
                new MemoryField("x_UnknVar_mirror_C_1", typeof(float), 1, 0x00),
                new MemoryField("x_UnknVar_mirror_C_2", typeof(float), 1, 0x00),
                new MemoryField("x_UnknVar_mirror_C_3", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_ca", typeof(byte), 68, 0x00), // 5620-5688
                new MemoryField("InputSteering_D", typeof(float), 1, 0x00),
                new MemoryField("x_UnknVar_inputThrot", typeof(float), 1, 0x00), // 5692
                new MemoryField("x_UnknVar_inputBrake", typeof(float), 1, 0x00), // 5696
                new MemoryField("x_UnknVar_carIsMoving", typeof(float), 1, 0x00), // 5700
                new MemoryField("x_UnknVar_carA", typeof(float), 1, 0x00), // 5704-5708
                //new MemoryField("car_SteeringWheelAxis", typeof(float), 3, 0x00),
                new MemoryField("car_SteeringWheelAxis_1", typeof(float), 1, 0x00),
                new MemoryField("car_SteeringWheelAxis_2", typeof(float), 1, 0x00),
                new MemoryField("car_SteeringWheelAxis_3", typeof(float), 1, 0x00),
                //new MemoryField("car_FlowCenter", typeof(float), 3, 0x00),
                new MemoryField("car_FlowCenter_1", typeof(float), 1, 0x00),
                new MemoryField("car_FlowCenter_2", typeof(float), 1, 0x00),
                new MemoryField("car_FlowCenter_3", typeof(float), 1, 0x00),
                //new MemoryField("Coords_C", typeof(float), 3, 0x00),
                new MemoryField("Coords_C_1", typeof(float), 1, 0x00),
                new MemoryField("Coords_C_2", typeof(float), 1, 0x00),
                new MemoryField("Coords_C_3", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_da", typeof(byte), 16, 0x00), // 5744-5760
                new MemoryField("x_UnknVar_da", typeof(byte), 1, 0x00),
                new MemoryField("Brakelight_A", typeof(byte), 1, 0x00),
                new MemoryField("Brakelight_B", typeof(byte), 1, 0x00),
                new MemoryField("x_UnknVar_db", typeof(byte), 1, 0x00),
                new MemoryField("x_UnknVar_dc", typeof(byte), 1, 0x00),
                new MemoryField("Headlight_A", typeof(byte), 1, 0x00),
                new MemoryField("Headlight_B", typeof(byte), 1, 0x00),
                new MemoryField("x_UnknVar_dd", typeof(byte), 1, 0x00),
                new MemoryField("x_Unkn_db", typeof(byte), 8, 0x00), // 5768-5776 = 4*FF
                new MemoryField("Pitlimiter_Rainlight", typeof(Int32), 1, 0x00),
                new MemoryField("x_Unkn_dc", typeof(byte), 24, 0x00), // 5780-5804
                //new MemoryField("hdc_FeelerFrontLeft", typeof(float), 3, 0x00),
                new MemoryField("hdc_FeelerFrontLeft_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerFrontLeft_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerFrontLeft_3", typeof(float), 1, 0x00),
                new MemoryField("FeelerFL_Unkn", typeof(byte), 16, 0x00), // 5816
                //new MemoryField("hdc_FeelerFrontRight", typeof(float), 3, 0x00),
                new MemoryField("hdc_FeelerFrontRight_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerFrontRight_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerFrontRight_3", typeof(float), 1, 0x00),
                new MemoryField("FeelerFR_Unkn", typeof(byte), 16, 0x00), // 5844
                //new MemoryField("hdc_FeelerRearLeft", typeof(float), 3, 0x00),
                new MemoryField("hdc_FeelerRearLeft_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerRearLeft_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerRearLeft_3", typeof(float), 1, 0x00),
                new MemoryField("FeelerRL_Unkn", typeof(byte), 16, 0x00), // 5872
                //new MemoryField("hdc_FeelerRearRight", typeof(float), 3, 0x00),
                new MemoryField("hdc_FeelerRearRight_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerRearRight_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerRearRight_3", typeof(float), 1, 0x00),
                new MemoryField("FeelerR_Unkn", typeof(byte), 16, 0x00), // 5900
                //new MemoryField("hdc_FeelerFront", typeof(float), 3, 0x00),
                new MemoryField("hdc_FeelerFront_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerFront_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerFront_3", typeof(float), 1, 0x00),
                new MemoryField("FeelerFrnt_Unkn", typeof(byte), 16, 0x00), // 5928
                //new MemoryField("hdc_FeelerRear", typeof(float), 3, 0x00),
                new MemoryField("hdc_FeelerRear_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerRear_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerRear_3", typeof(float), 1, 0x00),
                new MemoryField("FeelerRear_Unkn", typeof(byte), 16, 0x00), // 5956
                //new MemoryField("hdc_FeelerRight", typeof(float), 3, 0x00),
                new MemoryField("hdc_FeelerRight_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerRight_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerRight_3", typeof(float), 1, 0x00),
                new MemoryField("FeelerR_Unkn", typeof(byte), 16, 0x00), // 5984
                //new MemoryField("hdc_FeelerLeft", typeof(float), 3, 0x00),
                new MemoryField("hdc_FeelerLeft_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerLeft_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerLeft_3", typeof(float), 1, 0x00),
                new MemoryField("FeelerL_Unkn", typeof(byte), 16, 0x00), // 6012
                //new MemoryField("hdc_FeelerTopFrontLeft", typeof(float), 3, 0x00),
                new MemoryField("hdc_FeelerTopFrontLeft_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerTopFrontLeft_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerTopFrontLeft_3", typeof(float), 1, 0x00),
                new MemoryField("FeelerFLT_Unkn", typeof(byte), 16, 0x00), // 6040
                //new MemoryField("hdc_FeelerTopFrontRight", typeof(float), 3, 0x00),
                new MemoryField("hdc_FeelerTopFrontRight_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerTopFrontRight_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerTopFrontRight_3", typeof(float), 1, 0x00),
                new MemoryField("FeelerFRT_Unkn", typeof(byte), 16, 0x00), // 6068
                //new MemoryField("hdc_FeelerTopRearLeft", typeof(float), 3, 0x00),
                new MemoryField("hdc_FeelerTopRearLeft_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerTopRearLeft_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerTopRearLeft_3", typeof(float), 1, 0x00),
                new MemoryField("FeelerRLT_Unkn", typeof(byte), 16, 0x00), // 6096
                //new MemoryField("hdc_FeelerTopRearRight", typeof(float), 3, 0x00),
                new MemoryField("hdc_FeelerTopRearRight_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerTopRearRight_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerTopRearRight_3", typeof(float), 1, 0x00),
                new MemoryField("FeelerRRT_Unkn", typeof(byte), 16, 0x00), // 6124
                //new MemoryField("hdc_FeelerBottom", typeof(float), 3, 0x00),
                new MemoryField("hdc_FeelerBottom_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerBottom_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerBottom_3", typeof(float), 1, 0x00),
                new MemoryField("FeelerBtm_Unkn", typeof(byte), 16, 0x00), // 6152
                //new MemoryField("hdc_FLUndertray", typeof(float), 3, 0x00),
                new MemoryField("hdc_FLUndertray_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FLUndertray_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FLUndertray_3", typeof(float), 1, 0x00),
                new MemoryField("UndertrayFL_Unkn", typeof(byte), 16, 0x00), // 6180
                //new MemoryField("hdc_FRUndertray", typeof(float), 3, 0x00),
                new MemoryField("hdc_FRUndertray_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FRUndertray_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FRUndertray_3", typeof(float), 1, 0x00),
                new MemoryField("UndertrayFR_Unkn", typeof(byte), 16, 0x00), // 6208
                //new MemoryField("hdc_RLUndertray", typeof(float), 3, 0x00),
                new MemoryField("hdc_RLUndertray_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_RLUndertray_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_RLUndertray_3", typeof(float), 1, 0x00),
                new MemoryField("UndertrayRL_Unkn", typeof(byte), 16, 0x00), // 6236
                //new MemoryField("hdc_RRUndertray", typeof(float), 3, 0x00),
                new MemoryField("hdc_RRUndertray_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_RRUndertray_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_RRUndertray_3", typeof(float), 1, 0x00),
                new MemoryField("UndertrayRR_Unkn", typeof(byte), 16, 0x00), // 6264
                //new MemoryField("hdc_UndertrayFour", typeof(float), 3, 0x00),
                new MemoryField("hdc_UndertrayFour_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_UndertrayFour_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_UndertrayFour_3", typeof(float), 1, 0x00),
                new MemoryField("UndertrayFour_Unkn", typeof(byte), 16, 0x00), // 6292
                //new MemoryField("hdc_UndertrayFive", typeof(float), 3, 0x00),
                new MemoryField("hdc_UndertrayFive_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_UndertrayFive_2", typeof(float), 2, 0x00),
                new MemoryField("hdc_UndertrayFive_3", typeof(float), 3, 0x00),
                new MemoryField("UndertrayFive_Unkn", typeof(byte), 16, 0x00), // 6320
                //new MemoryField("hdc_UndertraySix", typeof(float), 3, 0x00),
                new MemoryField("hdc_UndertraySix_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_UndertraySix_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_UndertraySix_3", typeof(float), 1, 0x00),
                new MemoryField("UndertraySix_Unkn", typeof(byte), 16, 0x00), // 6348
                //new MemoryField("hdc_UndertraySeven", typeof(float), 3, 0x00),
                new MemoryField("hdc_UndertraySeven_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_UndertraySeven_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_UndertraySeven_3", typeof(float), 1, 0x00),
                new MemoryField("UndertraySeven_Unkn", typeof(byte), 16, 0x00), // 6376
                //new MemoryField("hdc_UndertrayEight", typeof(float), 3, 0x00),
                new MemoryField("hdc_UndertrayEight_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_UndertrayEight_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_UndertrayEight_3", typeof(float), 1, 0x00),
                new MemoryField("UndertrayEight_Unkn", typeof(byte), 16, 0x00), // 6404
                //new MemoryField("hdc_UndertrayNine", typeof(float), 3, 0x00),
                new MemoryField("hdc_UndertrayNine_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_UndertrayNine_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_UndertrayNine_3", typeof(float), 1, 0x00),
                new MemoryField("UndertrayNine_Unkn", typeof(byte), 16, 0x00), // 6432
                //new MemoryField("hdc_UndertrayTen", typeof(float), 3, 0x00),
                new MemoryField("hdc_UndertrayTen_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_UndertrayTen_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_UndertrayTen_3", typeof(float), 1, 0x00),
                new MemoryField("UndertrayTen_Unkn", typeof(byte), 16, 0x00), // 6460
                //new MemoryField("hdc_UndertrayEleven", typeof(float), 3, 0x00),
                new MemoryField("hdc_UndertrayEleven_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_UndertrayEleven_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_UndertrayEleven_3", typeof(float), 1, 0x00),
                new MemoryField("UndertrayEleven_Unkn", typeof(byte), 16, 0x00), // 6488
                new MemoryField("x_Unkn_e", typeof(byte), 68, 0x00), // 6504-6572
                new MemoryField("Damage_PartCount", typeof(Int32), 1, 0x00),
                new MemoryField("Damage_PartCount_Unkn", typeof(byte), 4, 0x00),
                //new MemoryField("Damage_PartInfo", typeof(byte), 11, 0x00), // 6580-14808 perlen:748
                new MemoryField("x_Unkn_f", typeof(byte), 808, 0x00), // 14808-15616
                new MemoryField("Unkn_Timing_A", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_g", typeof(byte), 264, 0x00), // 15620-15884
                new MemoryField("Unkn_Timing_B", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_h", typeof(byte), 40, 0x00), // 15888-15928
                new MemoryField("hdc_AIMinPassesPerTick", typeof(Int32), 1, 0x00),
                new MemoryField("x_Unkn_A", typeof(byte), 56, 0x00), // 15932-15988
                //new MemoryField("Coords_D", typeof(float), 3, 0x00),
                new MemoryField("Coords_D_1", typeof(float), 1, 0x00),
                new MemoryField("Coords_D_2", typeof(float), 1, 0x00),
                new MemoryField("Coords_D_3", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_B", typeof(byte), 48, 0x00), // 16000-16048
                new MemoryField("hdc_Mass", typeof(float), 1, 0x00),
                new MemoryField("x_UnknVar_A", typeof(float), 1, 0x00), // 16052
                new MemoryField("x_UnknVar_B", typeof(float), 1, 0x00), // 16056
                //new MemoryField("hdc_Inertia", typeof(float), 3, 0x00),
                new MemoryField("hdc_Inertia_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_Inertia_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_Inertia_3", typeof(float), 1, 0x00),
                new MemoryField("x_UnknVar_C", typeof(float), 1, 0x00), // 16072
                new MemoryField("x_UnknVar_D", typeof(float), 1, 0x00),
                new MemoryField("x_UnknVar_E", typeof(float), 1, 0x00), // 16080
                new MemoryField("WeightPenalty", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_C", typeof(byte), 48, 0x00), // 16088-16136
                //new MemoryField("hdc_AITorqueStab", typeof(float), 3, 0x00),
                new MemoryField("hdc_AITorqueStab_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_AITorqueStab_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_AITorqueStab_3", typeof(float), 1, 0x00),
                new MemoryField("InputThrottle_B", typeof(float), 1, 0x00),
                new MemoryField("InputSteering_B", typeof(float), 1, 0x00),
                new MemoryField("InputBrake_A", typeof(float), 1, 0x00),
                new MemoryField("x_UnknVar_I", typeof(float), 1, 0x00), // 16160
                //new MemoryField("x_Unkn_D_Wheels", typeof(byte), 1008, 0x00), // 16164-17172
                new MemoryField("tyr_FL_AISens_FinalLoad", typeof(float), 1, 0x00), // 16336
                new MemoryField("TyreWear_FL", typeof(float), 1, 0x00),
                new MemoryField("tyr_FL_AIWear", typeof(float), 1, 0x00), // 16344
                new MemoryField("TyreWear_FR", typeof(float), 1, 0x00),
                new MemoryField("tyr_FR_AIWear", typeof(float), 1, 0x00), // 16596
                new MemoryField("TyreWear_RL", typeof(float), 1, 0x00),
                new MemoryField("tyr_RL_AIWear", typeof(float), 1, 0x00), // 16848
                new MemoryField("TyreWear_RR", typeof(float), 1, 0x00),
                new MemoryField("tyr_RR_AIWear", typeof(float), 1, 0x00), // 17100
                //new MemoryField("x_Unkn_D_Engine", typeof(byte), 348, 0x00), // 17172-17520
                new MemoryField("CurrentFuelLevel", typeof(float), 1, 0x00), // 17196
                new MemoryField("hdc_FuelRange_Max", typeof(float), 1, 0x00), // 17200
                new MemoryField("eng_FuelConsumption", typeof(float), 1, 0x00), // 17208
                new MemoryField("eng_FuelEstimate", typeof(float), 1, 0x00), // 17212
                //new MemoryField("eng_StarterTiming", typeof(float), 3, 0x00), // 17268 ---TO_VERIFY
                new MemoryField("eng_StarterTiming_1", typeof(float), 1, 0x00),
                new MemoryField("eng_StarterTiming_2", typeof(float), 1, 0x00),
                new MemoryField("eng_StarterTiming_3", typeof(float), 1, 0x00),
                //new MemoryField("hdc_FeelerFrontLeftTwo", typeof(float), 3, 0x00),
                new MemoryField("hdc_FeelerFrontLeftTwo_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerFrontLeftTwo_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerFrontLeftTwo_3", typeof(float), 1, 0x00),
                //new MemoryField("hdc_FeelerFrontRightTwo", typeof(float), 3, 0x00),
                new MemoryField("hdc_FeelerFrontRightTwo_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerFrontRightTwo_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerFrontRightTwo_3", typeof(float), 1, 0x00),
                //new MemoryField("hdc_FeelerRearLeftTwo", typeof(float), 3, 0x00),
                new MemoryField("hdc_FeelerRearLeftTwo_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerRearLeftTwo_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerRearLeftTwo_3", typeof(float), 1, 0x00),
                //new MemoryField("hdc_FeelerRearRightTwo", typeof(float), 3, 0x00),
                new MemoryField("hdc_FeelerRearRightTwo_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerRearRightTwo_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerRearRightTwo_3", typeof(float), 1, 0x00),
                //new MemoryField("hdc_FeelerFrontTwo", typeof(float), 3, 0x00),
                new MemoryField("hdc_FeelerFrontTwo_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerFrontTwo_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerFrontTwo_3", typeof(float), 1, 0x00),
                //new MemoryField("hdc_FeelerRearTwo", typeof(float), 3, 0x00),
                new MemoryField("hdc_FeelerRearTwo_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerRearTwo_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerRearTwo_3", typeof(float), 1, 0x00),
                //new MemoryField("hdc_FeelerLeftTwo", typeof(float), 1, 0x00), // Bug: Most feelers have 3 floats? hdc_FeelerRightTwo has 3 floats.
                new MemoryField("hdc_FeelerLeftTwo_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerLeftTwo_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerLeftTwo_3", typeof(float), 1, 0x00),
                //new MemoryField("hdc_FeelerRightTwo", typeof(float), 3, 0x00),
                new MemoryField("hdc_FeelerRightTwo_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerRightTwo_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerRightTwo_3", typeof(float), 1, 0x00),
                //new MemoryField("hdc_FeelerTopFrontLeftTwo", typeof(float), 3, 0x00),
                new MemoryField("hdc_FeelerTopFrontLeftTwo_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerTopFrontLeftTwo_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerTopFrontLeftTwo_3", typeof(float), 1, 0x00),
                //new MemoryField("hdc_FeelerTopFrontRightTwo", typeof(float), 3, 0x00),
                new MemoryField("hdc_FeelerTopFrontRightTwo_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerTopFrontRightTwo_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerTopFrontRightTwo_3", typeof(float), 1, 0x00),
                //new MemoryField("hdc_FeelerTopRearLeftTwo", typeof(float), 3, 0x00),
                new MemoryField("hdc_FeelerTopRearLeftTwo_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerTopRearLeftTwo_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerTopRearLeftTwo_3", typeof(float), 1, 0x00),
                //new MemoryField("hdc_FeelerTopRearRightTwo", typeof(float), 3, 0x00),
                new MemoryField("hdc_FeelerTopRearRightTwo_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerTopRearRightTwo_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerTopRearRightTwo_3", typeof(float), 1, 0x00),
                //new MemoryField("hdc_FeelerBottomTwo", typeof(float), 3, 0x00),
                new MemoryField("hdc_FeelerBottomTwo_1", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerBottomTwo_2", typeof(float), 1, 0x00),
                new MemoryField("hdc_FeelerBottomTwo_3", typeof(float), 1, 0x00),
                //new MemoryField("x_Unkn_D_End_A", typeof(float), 3, 0x00), // 17676-17688
                new MemoryField("x_Unkn_D_End_A_1", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_D_End_A_2", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_D_End_A_3", typeof(float), 1, 0x00),
                //new MemoryField("Coords_E", typeof(float), 3, 0x00),
                new MemoryField("Coords_E_1", typeof(float), 1, 0x00),
                new MemoryField("Coords_E_2", typeof(float), 1, 0x00),
                new MemoryField("Coords_E_3", typeof(float), 1, 0x00),
                //new MemoryField("x_Unkn_D_End_B", typeof(float), 2, 0x00),
                new MemoryField("x_Unkn_D_End_B_1", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_D_End_B_2", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_E_Null", typeof(byte), 344, 0x00), // 17708-18052
                new MemoryField("x_Unkn_F_Damage", typeof(byte), 1168, 0x00), // 18052-19220
                new MemoryField("x_Unkn_Ga", typeof(byte), 28, 0x00), // 19220-19248
                new MemoryField("x_Unkn_Ga_Var_A", typeof(Int32), 1, 0x00), // live variable
                new MemoryField("x_Unkn_Ga_Var_B", typeof(float), 1, 0x00), // 19252
                new MemoryField("x_Unkn_Ga_Var_C", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_Ga_Var_D", typeof(float), 1, 0x00), // 19260
                new MemoryField("x_Unkn_Ga_Null_A", typeof(byte), 8, 0x00),
                new MemoryField("MAR_ThisSlot", typeof(Int32), 1, 0x00),
                new MemoryField("x_Unkn_Ga_Var_E", typeof(Int32), 1, 0x00),
                new MemoryField("x_Unkn_Ga_Var_F", typeof(Int32), 1, 0x00),
                new MemoryField("x_Unkn_Ga_Var_G", typeof(Int32), 1, 0x00),
                new MemoryField("x_Unkn_Ga_Var_H", typeof(Int32), 1, 0x00),
                new MemoryField("x_Unkn_Ga_Var_I_sco", typeof(float), 1, 0x00), // 19292
                new MemoryField("x_Unkn_Ga_Var_J", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_Ga_Var_K_sco", typeof(float), 1, 0x00), // 19300
                new MemoryField("Timing_Laptime_A", typeof(float), 1, 0x00),
                new MemoryField("Timing_SectorOne_A", typeof(float), 1, 0x00),
                new MemoryField("Timing_SectorTwo_A", typeof(float), 1, 0x00),
                new MemoryField("QualPos_Forced", typeof(Int32), 1, 0x00), // 19316
                new MemoryField("QualBestLaptime", typeof(float), 1, 0x00), // 19320
                new MemoryField("QualPos_One", typeof(Int32), 1, 0x00), // 19324
                new MemoryField("QualPos_Two", typeof(Int32), 1, 0x00), // 19328
                new MemoryField("x_Unkn_Ga_Null_B", typeof(byte), 4, 0x00),
                new MemoryField("x_Unkn_Gb_FFed", typeof(byte), 40, 0x00), // 19336-19376
                new MemoryField("x_Unkn_Gb_Null_A", typeof(byte), 12, 0x00),
                new MemoryField("x_Unkn_Gb_Var_A", typeof(Int32), 1, 0x00), // 19388
                new MemoryField("x_Unkn_Gb_Null_B", typeof(byte), 36, 0x00), // 19392-19428
                new MemoryField("Timing_LapStartET_B", typeof(float), 1, 0x00), // 19432
                new MemoryField("Timing_Laptime_B", typeof(float), 1, 0x00),
                new MemoryField("Timing_SectorOne_B", typeof(float), 1, 0x00),
                new MemoryField("Timing_SectorTwo_B", typeof(float), 1, 0x00),
                new MemoryField("Timing_Laptime_C", typeof(float), 1, 0x00),
                new MemoryField("Timing_SectorOne_C", typeof(float), 1, 0x00),
                new MemoryField("Timing_SectorTwo_C", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_Gb_Var_C", typeof(float), 1, 0x00), // 19460
                new MemoryField("x_Unkn_Gb_Null_C", typeof(byte), 4, 0x00),
                new MemoryField("Timing_Laptime_D", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_Gb_Var_D", typeof(Int32), 1, 0x00), // 19472
                new MemoryField("x_Unkn_Gb_Var_E", typeof(Int32), 1, 0x00),
                new MemoryField("Timing_LapStartET_Log_MAR", typeof(Int32), 1, 0x00), // 19480
                new MemoryField("Timing_Laptime_Log_MAR", typeof(Int32), 1, 0x00),
                new MemoryField("Timing_SectorOne_Log_MAR", typeof(Int32), 1, 0x00),
                new MemoryField("Timing_SectorTwo_Log_MAR", typeof(Int32), 1, 0x00),
                new MemoryField("x_Unkn_Gb_MAR", typeof(Int32), 1, 0x00), // 19496
                new MemoryField("x_Unkn_Gb_Null_D", typeof(byte), 4, 0x00), // 19500-19504
                new MemoryField("CarAhead_MAR", typeof(Int32), 1, 0x00),
                new MemoryField("CarBehind_MAR", typeof(Int32), 1, 0x00),
                new MemoryField("x_Unkn_Gc", typeof(byte), 680, 0x00), // 19512-20192
                //new MemoryField("x_Unkn_Gc_MAR", typeof(Int32), 20, 0x00), // TODO: 20 Int32s in a row? Really? Or just 80 unknown bytes?
                new MemoryField("x_Unkn_Gc_MAR", typeof(byte), 20, 0x00),
                //new MemoryField("x_Unkn_Gc_float", typeof(float), 3, 0x00),
                new MemoryField("x_Unkn_Gc_float_1", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_Gc_float_2", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_Gc_float_3", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_Gd", typeof(byte), 8, 0x00), // 20284-20292
                new MemoryField("x_Unkn_Gd_float", typeof(float), 15, 0x00),
                new MemoryField("x_Unkn_Ge", typeof(byte), 108, 0x00), // 20352-20460
                new MemoryField("InputSteering_C", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_Ge_float_A", typeof(float), 1, 0x00), // 20464
                new MemoryField("InputThrottle_C", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_Ge_float_B", typeof(float), 1, 0x00), // 20472
                new MemoryField("InputBrake_B", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_Gf", typeof(byte), 20, 0x00), // 20480-20500
                new MemoryField("CurrentLapScoring", typeof(float), 1, 0x00),
                new MemoryField("x_UnknVar_sco_A", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_Gg", typeof(byte), 96, 0x00), // 20508-20604
                //new MemoryField("Coords_F", typeof(float), 3, 0x00),
                new MemoryField("Coords_F_1", typeof(float), 1, 0x00),
                new MemoryField("Coords_F_2", typeof(float), 1, 0x00),
                new MemoryField("Coords_F_3", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_Gh", typeof(byte), 28, 0x00), // 20616-20644
                new MemoryField("rcd_MinRacingSkill", typeof(float), 1, 0x00),
                new MemoryField("x_UnknVar_RCD_A", typeof(float), 1, 0x00), // 20648
                new MemoryField("x_UnknVar_RCD_B", typeof(float), 1, 0x00), // 20652
                new MemoryField("rcd_StartStalls", typeof(float), 1, 0x00),
                new MemoryField("x_UnknVar_RCD_C", typeof(float), 1, 0x00), // 20660
                new MemoryField("x_UnknVar_RCD_D", typeof(float), 1, 0x00),
                new MemoryField("x_UnknVar_RCD_E", typeof(float), 1, 0x00),
                new MemoryField("x_UnknVar_RCD_F", typeof(float), 1, 0x00), // 20672
                new MemoryField("rcd_Crash", typeof(float), 1, 0x00),
                new MemoryField("rcd_Composure", typeof(float), 1, 0x00),
                new MemoryField("x_UnknVar_RCD_G", typeof(float), 1, 0x00), // 20684
                new MemoryField("rcd_CompletedLaps", typeof(float), 1, 0x00),
                new MemoryField("rcd_Script", typeof(byte), 20, 0x00),
                new MemoryField("x_UnknVar_RCD_H", typeof(float), 1, 0x00), // 20712
                new MemoryField("rcd_TrackAggression", typeof(float), 1, 0x00),
                new MemoryField("x_UnknVar_RCD_I", typeof(Int32), 1, 0x00), // 20720
                new MemoryField("rcd_CorneringAdd", typeof(float), 1, 0x00),
                new MemoryField("rcd_CorneringMult", typeof(float), 1, 0x00),
                new MemoryField("rcd_RaceColdBrainMin", typeof(float), 1, 0x00),
                new MemoryField("rcd_RaceColdBrainTime", typeof(float), 1, 0x00),
                new MemoryField("rcd_QualColdBrainMin", typeof(float), 1, 0x00),
                new MemoryField("rcd_QualColdBrainTime", typeof(float), 1, 0x00),
                new MemoryField("x_UnknVar_RCD_J", typeof(float), 1, 0x00), // 20748
                new MemoryField("rcd_TCGripThreshold", typeof(float), 1, 0x00),
                new MemoryField("rcd_TCThrottleFract", typeof(float), 1, 0x00),
                new MemoryField("rcd_TCResponse", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_H", typeof(byte), 56, 0x00), // 20764-20820
                new MemoryField("rcd_QualColdBrainMinRpt", typeof(float), 1, 0x00),
                new MemoryField("rcd_QualColdBrainTimeRpt", typeof(float), 1, 0x00),
                new MemoryField("x_Unkn_I", typeof(byte), 740, 0x00), // 20828-21568
                new MemoryField("x_UnknVar_J", typeof(float), 1, 0x00), // 21568
                new MemoryField("x_Unkn_J", typeof(byte), 4, 0x00),
                new MemoryField("NameFull_One", typeof(byte), 64, 0x00),
                new MemoryField("CountryFull_One", typeof(byte), 24, 0x00),
                new MemoryField("NameAbbrev_One", typeof(byte), 16, 0x00),
                new MemoryField("x_UnknVar_K", typeof(float), 1, 0x00), // 21680 = 32.25
                new MemoryField("NameFull_Two", typeof(byte), 64, 0x00),
                new MemoryField("CountryFull_Two", typeof(byte), 24, 0x00),
                new MemoryField("NameAbbrev_Two", typeof(byte), 16, 0x00),
                new MemoryField("x_UnknVar_L", typeof(byte), 24, 0x00), // 21788-21812
                new MemoryField("x_UnknVar_M", typeof(Int32), 1, 0x00), // 21812
                new MemoryField("car_Description", typeof(byte), 64, 0x00),
                new MemoryField("car_Team", typeof(byte), 64, 0x00),
                new MemoryField("car_PitGroup", typeof(byte), 64, 0x00),
                new MemoryField("car_Manufacturer", typeof(byte), 64, 0x00),
                new MemoryField("car_Engine", typeof(byte), 8, 0x00),
                new MemoryField("car_Number_str", typeof(byte), 8, 0x00),
                new MemoryField("car_Number_int", typeof(Int32), 1, 0x00),
                new MemoryField("car_FilePath", typeof(byte), 128, 0x00),
                new MemoryField("svm_FilePath", typeof(byte), 128, 0x00),
                new MemoryField("x_Unkn_L", typeof(byte), 128, 0x00), // 22348-22476
                new MemoryField("x_UnknVar_EndA", typeof(Int32), 1, 0x00), // 22476
                new MemoryField("x_UnknVar_EndB", typeof(Int32), 1, 0x00), // 22480
                new MemoryField("x_Unkn_EndA", typeof(byte), 72, 0x00), // 22484-22556
                new MemoryField("x_UnknVar_EndC", typeof(Int32), 1, 0x00), // 22556
                new MemoryField("x_UnknVar_EndD", typeof(Int32), 1, 0x00), // 22560
                new MemoryField("x_Unkn_EndB", typeof(byte), 8, 0x00), // 22564-22572
                new MemoryField("x_UnknVar_EndE", typeof(byte), 4, 0x00), // 22572
                new MemoryField("x_Unkn_EndC", typeof(byte), 12, 0x00), // 22576-22588
                new MemoryField("x_UnknVar_EndF", typeof(Int32), 1, 0x00), // 22588
                new MemoryField("x_Unkn_EndD", typeof(byte), 32, 0x00) // 22592-22624
            ];
            foreach (MemoryField field in Fields)
            {
                Region.AddField(field);
            }

           
        }
    }
}
