using System;
using System.Collections.Generic;

namespace FitParser.Core
{
    // Definitions for record structure and field types

    public class FieldDecl
    {
        public FieldDecl(byte fieldNumber, string fieldName)
        {
            FieldNumber = fieldNumber;
            FieldName = fieldName;
        }

        public FieldDecl(byte fieldNumber, string fieldName, bool isEnum, bool isArray) : this(fieldNumber, fieldName)
        {
            IsEnum = isEnum;
            IsArray = isArray;
        }

        public readonly string FieldName;

        public readonly byte FieldNumber;

        public readonly bool IsEnum;

        public readonly bool IsArray;

        public static implicit operator byte(FieldDecl field)
        {
            return field.FieldNumber;
        }
    }

    public class FieldDecls
    {
        public readonly LinkedList<FieldDecl>[] Declarations = new LinkedList<FieldDecl>[256];

        public FieldDecl DeclareField(byte fieldNumber, string fieldName, bool isEnum, bool isArray)
        {
            FieldDecl result = new FieldDecl(fieldNumber, fieldName, isEnum, isArray);

            if (Declarations[fieldNumber] != null)
            {
                Declarations[fieldNumber].AddLast(result);
            }
            else
            {
                Declarations[fieldNumber] = new LinkedList<FieldDecl>();
                Declarations[fieldNumber].AddLast(result);
            }

            return result;
        }
        
        public FieldDecl DeclareField(byte fieldNumber, string fieldName)
        {
            return DeclareField(fieldNumber, fieldName, false, false);
        }
    }

    public class RecordDef 
    {
        public static FieldDecls Declarations = new FieldDecls();

        public static readonly FieldDecl PositionLat = Declarations.DeclareField(0, "PositionLat");
        public static readonly FieldDecl PositionLong = Declarations.DeclareField(1, "PositionLong");
        public static readonly FieldDecl Altitude = Declarations.DeclareField(2, "Altitude");
        public static readonly FieldDecl HeartRate = Declarations.DeclareField(3, "HeartRate");
        public static readonly FieldDecl Cadence = Declarations.DeclareField(4, "Cadence");
        public static readonly FieldDecl Distance = Declarations.DeclareField(5, "Distance");
        public static readonly FieldDecl Speed = Declarations.DeclareField(6, "Speed");
        public static readonly FieldDecl Power = Declarations.DeclareField(7, "Power");
        public static readonly FieldDecl CompressedSpeedDistance = Declarations.DeclareField(8, "CompressedSpeedDistance", false, true);
        public static readonly FieldDecl Grade = Declarations.DeclareField(9, "Grade");
        public static readonly FieldDecl Resistance = Declarations.DeclareField(10, "Resistance");
        public static readonly FieldDecl TimeFromCourse = Declarations.DeclareField(11, "TimeFromCourse");
        public static readonly FieldDecl CycleLength = Declarations.DeclareField(12, "CycleLength");
        public static readonly FieldDecl Temperature = Declarations.DeclareField(13, "Temperature");
        public static readonly FieldDecl Speed1s = Declarations.DeclareField(17, "Speed1s", false, true);
        public static readonly FieldDecl Cycles = Declarations.DeclareField(18, "Cycles");
        public static readonly FieldDecl TotalCycles = Declarations.DeclareField(19, "TotalCycles");
        public static readonly FieldDecl CompressedAccumulatedPower = Declarations.DeclareField(28, "CompressedAccumulatedPower");
        public static readonly FieldDecl AccumulatedPower = Declarations.DeclareField(29, "AccumulatedPower");
        public static readonly FieldDecl LeftRightBalance = Declarations.DeclareField(30, "LeftRightBalance");
        public static readonly FieldDecl GpsAccuracy = Declarations.DeclareField(31, "GpsAccuracy");
        public static readonly FieldDecl VerticalSpeed = Declarations.DeclareField(32, "VerticalSpeed");
        public static readonly FieldDecl Calories = Declarations.DeclareField(33, "Calories");
        public static readonly FieldDecl VerticalOscillation = Declarations.DeclareField(39, "VerticalOscillation");
        public static readonly FieldDecl StanceTimePercent = Declarations.DeclareField(40, "StanceTimePercent");
        public static readonly FieldDecl StanceTime = Declarations.DeclareField(41, "StanceTime");
        public static readonly FieldDecl ActivityType = Declarations.DeclareField(42, "ActivityType", true, false);
        public static readonly FieldDecl LeftTorqueEffectiveness = Declarations.DeclareField(43, "LeftTorqueEffectiveness");
        public static readonly FieldDecl RightTorqueEffectiveness = Declarations.DeclareField(44, "RightTorqueEffectiveness");
        public static readonly FieldDecl LeftPedalSmoothness = Declarations.DeclareField(45, "LeftPedalSmoothness");
        public static readonly FieldDecl RightPedalSmoothness = Declarations.DeclareField(46, "RightPedalSmoothness");
        public static readonly FieldDecl CombinedPedalSmoothness = Declarations.DeclareField(47, "CombinedPedalSmoothness");
        public static readonly FieldDecl Time128 = Declarations.DeclareField(48, "Time128");
        public static readonly FieldDecl StrokeType = Declarations.DeclareField(49, "StrokeType", true, false);
        public static readonly FieldDecl Zone = Declarations.DeclareField(50, "Zone");
        public static readonly FieldDecl BallSpeed = Declarations.DeclareField(51, "BallSpeed");
        public static readonly FieldDecl Cadence256 = Declarations.DeclareField(52, "Cadence256");
        public static readonly FieldDecl TotalHemoglobinConc = Declarations.DeclareField(54, "TotalHemoglobinConc");
        public static readonly FieldDecl TotalHemoglobinConcMin = Declarations.DeclareField(55, "TotalHemoglobinConcMin");
        public static readonly FieldDecl TotalHemoglobinConcMax = Declarations.DeclareField(56, "TotalHemoglobinConcMax");
        public static readonly FieldDecl SaturatedHemoglobinPercent = Declarations.DeclareField(57, "SaturatedHemoglobinPercent");
        public static readonly FieldDecl SaturatedHemoglobinPercentMin = Declarations.DeclareField(58, "SaturatedHemoglobinPercentMin");
        public static readonly FieldDecl SaturatedHemoglobinPercentMax = Declarations.DeclareField(59, "SaturatedHemoglobinPercentMax");
        public static readonly FieldDecl DeviceIndex = Declarations.DeclareField(62, "DeviceIndex");
        public static readonly FieldDecl TimeStamp = Declarations.DeclareField(253, "TimeStamp");
    }

    public class EventDef 
    {
        public static FieldDecls Declarations = new FieldDecls();

        public static readonly FieldDecl Event = Declarations.DeclareField(0, "Event", true, false);
        public static readonly FieldDecl EventType = Declarations.DeclareField(1, "EventType", true, false);
        public static readonly FieldDecl Data16 = Declarations.DeclareField(2, "Data16");
        public static readonly FieldDecl Data = Declarations.DeclareField(3, "Data");
        public static readonly FieldDecl TimerTrigger = Declarations.DeclareField(3, "TimerTrigger", true, false); // alias
        public static readonly FieldDecl CoursePointIndex = Declarations.DeclareField(3, "CoursePointIndex"); // alias
        public static readonly FieldDecl BatteryLevel = Declarations.DeclareField(3, "BatteryLevel"); // alias
        public static readonly FieldDecl VirtualPartnerSpeed = Declarations.DeclareField(3, "VirtualPartnerSpeed"); // alias
        public static readonly FieldDecl HrHighAlert = Declarations.DeclareField(3, "HrHighAlert"); // alias
        public static readonly FieldDecl HrLowAlert = Declarations.DeclareField(3, "HrLowAlert"); // alias
        public static readonly FieldDecl SpeedHighAlert = Declarations.DeclareField(3, "SpeedHighAlert"); // alias
        public static readonly FieldDecl SpeedLowAlert = Declarations.DeclareField(3, "SpeedLowAlert"); // alias
        public static readonly FieldDecl CadHighAlert = Declarations.DeclareField(3, "CadHighAlert"); // alias
        public static readonly FieldDecl CadLowAlert = Declarations.DeclareField(3, "CadLowAlert"); // alias
        public static readonly FieldDecl PowerHighAlert = Declarations.DeclareField(3, "PowerHighAlert"); // alias
        public static readonly FieldDecl PowerLowAlert = Declarations.DeclareField(3, "PowerLowAlert"); // alias
        public static readonly FieldDecl TimeDurationAlert = Declarations.DeclareField(3, "TimeDurationAlert"); // alias
        public static readonly FieldDecl DistanceDurationAlert = Declarations.DeclareField(3, "DistanceDurationAlert"); // alias
        public static readonly FieldDecl CalorieDurationAlert = Declarations.DeclareField(3, "CalorieDurationAlert"); // alias
        public static readonly FieldDecl FitnessEquipmentState = Declarations.DeclareField(3, "FitnessEquipmentState", true, false); // alias
        public static readonly FieldDecl SportPoint = Declarations.DeclareField(3, "SportPoint"); // alias
        public static readonly FieldDecl GearChangeData = Declarations.DeclareField(3, "GearChangeData"); // alias
        public static readonly FieldDecl EventGroup = Declarations.DeclareField(4, "EventGroup", true, false); 
        public static readonly FieldDecl Score = Declarations.DeclareField(7, "Score"); 
        public static readonly FieldDecl OpponentScore = Declarations.DeclareField(8, "OpponentScore"); 
        public static readonly FieldDecl FrontGearNum = Declarations.DeclareField(9, "FrontGearNum"); 
        public static readonly FieldDecl FrontGear = Declarations.DeclareField(10, "FrontGear"); 
        public static readonly FieldDecl RearGearNum = Declarations.DeclareField(11, "RearGearNum"); 
        public static readonly FieldDecl RearGear = Declarations.DeclareField(12, "RearGear"); 
        public static readonly FieldDecl TimeStamp = Declarations.DeclareField(253, "TimeStamp");
    }

    public static class LapDef 
    {
        public static FieldDecls Declarations = new FieldDecls();

        public static readonly FieldDecl Event = Declarations.DeclareField(0, "Event", true, false);
        public static readonly FieldDecl EventType = Declarations.DeclareField(1, "EventType", true, false);
        public static readonly FieldDecl StartTime = Declarations.DeclareField(2, "StartTime");
        public static readonly FieldDecl StartPositionLat = Declarations.DeclareField(3, "StartPositionLat");
        public static readonly FieldDecl StartPositionLong = Declarations.DeclareField(4, "StartPositionLong");
        public static readonly FieldDecl EndPositionLat = Declarations.DeclareField(5, "EndPositionLat");
        public static readonly FieldDecl EndPositionLong = Declarations.DeclareField(6, "EndPositionLong");
        public static readonly FieldDecl TotalElapsedTime = Declarations.DeclareField(7, "TotalElapsedTime");
        public static readonly FieldDecl TotalTimerTime = Declarations.DeclareField(8, "TotalTimerTime");
        public static readonly FieldDecl TotalDistance = Declarations.DeclareField(9, "TotalDistance");
        public static readonly FieldDecl TotalCycles = Declarations.DeclareField(10, "TotalCycles");
        public static readonly FieldDecl TotalStrides = Declarations.DeclareField(10, "TotalStrides"); // alias
        public static readonly FieldDecl TotalCalories = Declarations.DeclareField(11, "TotalCalories");
        public static readonly FieldDecl TotalFatCalories = Declarations.DeclareField(12, "TotalFatCalories");
        public static readonly FieldDecl AvgSpeed = Declarations.DeclareField(13, "AvgSpeed");
        public static readonly FieldDecl MaxSpeed = Declarations.DeclareField(14, "MaxSpeed");
        public static readonly FieldDecl AvgHeartRate = Declarations.DeclareField(15, "AvgHeartRate");
        public static readonly FieldDecl MaxHeartRate = Declarations.DeclareField(16, "MaxHeartRate");
        public static readonly FieldDecl AvgCadence = Declarations.DeclareField(17, "AvgCadence");
        public static readonly FieldDecl AvgRunningCadence = Declarations.DeclareField(17, "AvgRunningCadence"); // alias
        public static readonly FieldDecl MaxCadence = Declarations.DeclareField(18, "MaxCadence");
        public static readonly FieldDecl MaxRunningCadence = Declarations.DeclareField(18, "MaxRunningCadence"); // alias
        public static readonly FieldDecl AvgPower = Declarations.DeclareField(19, "AvgPower");
        public static readonly FieldDecl MaxPower = Declarations.DeclareField(20, "MaxPower");
        public static readonly FieldDecl TotalAscent = Declarations.DeclareField(21, "TotalAscent");
        public static readonly FieldDecl TotalDescent = Declarations.DeclareField(22, "TotalDescent");
        public static readonly FieldDecl Intensity = Declarations.DeclareField(23, "Intensity");
        public static readonly FieldDecl LapTrigger = Declarations.DeclareField(24, "LapTrigger", true, false);
        public static readonly FieldDecl Sport = Declarations.DeclareField(25, "Sport", true, false);
        public static readonly FieldDecl EventGroup = Declarations.DeclareField(26, "EventGroup");
        // Coordinates of the NE and SW corners of the lap. These are not documented by the SDK, will see if they are right.
        public static readonly FieldDecl NecLat = Declarations.DeclareField(27, "NecLat");
        public static readonly FieldDecl NecLong = Declarations.DeclareField(28, "NecLong");
        public static readonly FieldDecl SwcLat = Declarations.DeclareField(29, "SwcLat");
        public static readonly FieldDecl SwcLong = Declarations.DeclareField(30, "SwcLong");
        public static readonly FieldDecl NumLengths = Declarations.DeclareField(32, "NumLengths");
        public static readonly FieldDecl NormalizedPower = Declarations.DeclareField(33, "NormalizedPower");
        public static readonly FieldDecl LeftRightBalance = Declarations.DeclareField(34, "LeftRightBalance");
        public static readonly FieldDecl FirstLengthIndex = Declarations.DeclareField(35, "FirstLengthIndex");
        public static readonly FieldDecl AvgStrokeDistance = Declarations.DeclareField(37, "AvgStrokeDistance");
        public static readonly FieldDecl SwimStroke = Declarations.DeclareField(38, "SwimStroke", true, false);
        public static readonly FieldDecl SubSport = Declarations.DeclareField(39, "SubSport", true, false);
        public static readonly FieldDecl NumActiveLengths = Declarations.DeclareField(40, "NumActiveLengths");
        public static readonly FieldDecl TotalWork = Declarations.DeclareField(41, "TotalWork");
        public static readonly FieldDecl AvgAltitude = Declarations.DeclareField(42, "AvgAltitude");
        public static readonly FieldDecl MaxAltitude = Declarations.DeclareField(43, "MaxAltitude");
        public static readonly FieldDecl GpsAccuracy = Declarations.DeclareField(44, "GpsAccuracy");
        public static readonly FieldDecl AvgGrade = Declarations.DeclareField(45, "AvgGrade");
        public static readonly FieldDecl AvgPosGrade = Declarations.DeclareField(46, "AvgPosGrade");
        public static readonly FieldDecl AvgNegGrade = Declarations.DeclareField(47, "AvgNegGrade");
        public static readonly FieldDecl MaxPosGrade = Declarations.DeclareField(48, "MaxPosGrade");
        public static readonly FieldDecl MaxNegGrade = Declarations.DeclareField(49, "MaxNegGrade");
        public static readonly FieldDecl AvgTemperature = Declarations.DeclareField(50, "AvgTemperature");
        public static readonly FieldDecl MaxTemperature = Declarations.DeclareField(51, "MaxTemperature");
        public static readonly FieldDecl TotalMovingTime = Declarations.DeclareField(52, "TotalMovingTime");
        public static readonly FieldDecl AvgPosVerticalSpeed = Declarations.DeclareField(53, "AvgPosVerticalSpeed");
        public static readonly FieldDecl AvgNegVerticalSpeed = Declarations.DeclareField(54, "AvgNegVerticalSpeed");
        public static readonly FieldDecl MaxPosVerticalSpeed = Declarations.DeclareField(55, "MaxPosVerticalSpeed");
        public static readonly FieldDecl MaxNegVerticalSpeed = Declarations.DeclareField(56, "MaxNegVerticalSpeed");
        public static readonly FieldDecl TimeInHrZone = Declarations.DeclareField(57, "TimeInHrZone", false, true);
        public static readonly FieldDecl TimeInSpeedZone = Declarations.DeclareField(58, "TimeInSpeedZone", false, true);
        public static readonly FieldDecl TimeInCadenceZone = Declarations.DeclareField(59, "TimeInCadenceZone", false, true);
        public static readonly FieldDecl TimeInPowerZone = Declarations.DeclareField(60, "TimeInPowerZone", false, true);
        public static readonly FieldDecl RepetitionNum = Declarations.DeclareField(61, "RepetitionNum");
        public static readonly FieldDecl MinAltitude = Declarations.DeclareField(62, "MinAltitude");
        public static readonly FieldDecl MinHeartRate = Declarations.DeclareField(63, "MinHeartRate");
        public static readonly FieldDecl WktStepIndex = Declarations.DeclareField(71, "WktStepIndex");
        public static readonly FieldDecl OpponentScore = Declarations.DeclareField(74, "OpponentScore");
        public static readonly FieldDecl StrokeCount = Declarations.DeclareField(75, "StrokeCount", false, true);
        public static readonly FieldDecl ZoneCount = Declarations.DeclareField(76, "ZoneCount", false, true);
        public static readonly FieldDecl AvgVerticalOscillation = Declarations.DeclareField(77, "AvgVerticalOscillation");
        public static readonly FieldDecl AvgStanceTimePercent = Declarations.DeclareField(78, "AvgStanceTimePercent");
        public static readonly FieldDecl AvgStanceTime = Declarations.DeclareField(79, "AvgStanceTime");
        public static readonly FieldDecl AvgFractionalCadence = Declarations.DeclareField(80, "AvgFractionalCadence");
        public static readonly FieldDecl MaxFractionalCadence = Declarations.DeclareField(81, "MaxFractionalCadence");
        public static readonly FieldDecl TotalFractionalCycles = Declarations.DeclareField(82, "TotalFractionalCycles");
        public static readonly FieldDecl PlayerScore = Declarations.DeclareField(83, "PlayerScore");
        public static readonly FieldDecl AvgTotalHemoglobinConc = Declarations.DeclareField(84, "AvgTotalHemoglobinConc", false, true);
        public static readonly FieldDecl MinTotalHemoglobinConc = Declarations.DeclareField(85, "MinTotalHemoglobinConc", false, true);
        public static readonly FieldDecl MaxTotalHemoglobinConc = Declarations.DeclareField(86, "MaxTotalHemoglobinConc", false, true);
        public static readonly FieldDecl AvgSaturatedHemoglobinPercent = Declarations.DeclareField(87, "AvgSaturatedHemoglobinPercent", false, true);
        public static readonly FieldDecl MinSaturatedHemoglobinPercent = Declarations.DeclareField(88, "MinSaturatedHemoglobinPercent", false, true);
        public static readonly FieldDecl MaxSaturatedHemoglobinPercent = Declarations.DeclareField(89, "MaxSaturatedHemoglobinPercent", false, true);
        public static readonly FieldDecl AvgLeftTorqueEffectiveness = Declarations.DeclareField(91, "AvgLeftTorqueEffectiveness");
        public static readonly FieldDecl AvgRightTorqueEffectiveness = Declarations.DeclareField(92, "AvgRightTorqueEffectiveness");
        public static readonly FieldDecl AvgLeftPedalSmoothness = Declarations.DeclareField(93, "AvgLeftPedalSmoothness");
        public static readonly FieldDecl AvgRightPedalSmoothness = Declarations.DeclareField(94, "AvgRightPedalSmoothness");
        public static readonly FieldDecl AvgCombinedPedalSmoothness = Declarations.DeclareField(95, "AvgCombinedPedalSmoothness");
        public static readonly FieldDecl TimeStamp = Declarations.DeclareField(253, "TimeStamp");
        public static readonly FieldDecl MessageIndex = Declarations.DeclareField(254, "MessageIndex");
    }

    public static class DeviceInfoDef
    {
        public static FieldDecls Declarations = new FieldDecls();

        public static readonly FieldDecl DeviceIndex = Declarations.DeclareField(0, "DeviceIndex");
        public static readonly FieldDecl DeviceType = Declarations.DeclareField(1, "DeviceType");
        public static readonly FieldDecl AntplusDeviceType = Declarations.DeclareField(1, "AntplusDeviceType"); // alias
        public static readonly FieldDecl AntDeviceType = Declarations.DeclareField(1, "AntDeviceType"); // alias
        public static readonly FieldDecl Manufacturer = Declarations.DeclareField(2, "Manufacturer"); 
        public static readonly FieldDecl SerialNumber = Declarations.DeclareField(3, "SerialNumber"); 
        public static readonly FieldDecl Product = Declarations.DeclareField(4, "Product"); 
        public static readonly FieldDecl SoftwareVersion = Declarations.DeclareField(5, "SoftwareVersion"); 
        public static readonly FieldDecl HardwareVersion = Declarations.DeclareField(6, "HardwareVersion"); 
        public static readonly FieldDecl CumOperatingTime = Declarations.DeclareField(7, "CumOperatingTime"); 
        public static readonly FieldDecl BatteryVoltage = Declarations.DeclareField(10, "BatteryVoltage"); 
        public static readonly FieldDecl BatteryStatus = Declarations.DeclareField(11, "BatteryStatus"); 
        public static readonly FieldDecl SensorPosition = Declarations.DeclareField(18, "SensorPosition", true, false); 
        public static readonly FieldDecl Descriptor = Declarations.DeclareField(19, "Descriptor", false, true);  // this is a new type - byte array
        public static readonly FieldDecl AntTransmissionType = Declarations.DeclareField(20, "AntTransmissionType"); 
        public static readonly FieldDecl AntDeviceNumber = Declarations.DeclareField(21, "AntDeviceNumber"); 
        public static readonly FieldDecl AntNetwork = Declarations.DeclareField(22, "AntNetwork", true, false); 
        public static readonly FieldDecl SourceType = Declarations.DeclareField(25, "SourceType", true, false); 
        public static readonly FieldDecl TimeStamp = Declarations.DeclareField(253, "TimeStamp");
    }

    public static class ActivityDef
    {
        public static FieldDecls Declarations = new FieldDecls();

        public static readonly FieldDecl TotalTimerTime = Declarations.DeclareField(0, "TotalTimerTime");
        public static readonly FieldDecl NumSessions = Declarations.DeclareField(1, "NumSessions");
        public static readonly FieldDecl Type = Declarations.DeclareField(2, "Type", true, false);
        public static readonly FieldDecl Event = Declarations.DeclareField(3, "Event", true, false);
        public static readonly FieldDecl EventType = Declarations.DeclareField(4, "EventType", true, false);
        public static readonly FieldDecl LocalTimestamp = Declarations.DeclareField(5, "LocalTimestamp");
        public static readonly FieldDecl EventGroup = Declarations.DeclareField(6, "EventGroup");
        public static readonly FieldDecl TimeStamp = Declarations.DeclareField(253, "TimeStamp");
    }

    public static class FileIdDef
    {
        public static FieldDecls Declarations = new FieldDecls();

        public static readonly FieldDecl Type = Declarations.DeclareField(0, "Type", true, false);
        public static readonly FieldDecl Manufacturer = Declarations.DeclareField(1, "Manufacturer");
        public static readonly FieldDecl Product = Declarations.DeclareField(2, "Product");
        public static readonly FieldDecl GarminProduct = Declarations.DeclareField(2, "GarminProduct"); // alias
        public static readonly FieldDecl SerialNumber = Declarations.DeclareField(3, "SerialNumber");
        public static readonly FieldDecl TimeCreated = Declarations.DeclareField(4, "TimeCreated");
        public static readonly FieldDecl Number = Declarations.DeclareField(5, "Number");
    }

    public static class FileCreatorDef
    {
        public static FieldDecls Declarations = new FieldDecls();

        public static readonly FieldDecl SoftwareVersion = Declarations.DeclareField(0, "SoftwareVersion");
        public static readonly FieldDecl HardwareVersion = Declarations.DeclareField(1, "HardwareVersion");
    }

    public static class SessionDef
    {
        public static FieldDecls Declarations = new FieldDecls();

        public static readonly FieldDecl Event = Declarations.DeclareField(0, "Event", true, false);
        public static readonly FieldDecl EventType = Declarations.DeclareField(1, "EventType", true, false);
        public static readonly FieldDecl StartTime = Declarations.DeclareField(2, "StartTime");
        public static readonly FieldDecl StartPositionLat = Declarations.DeclareField(3, "StartPositionLat");
        public static readonly FieldDecl StartPositionLong = Declarations.DeclareField(4, "StartPositionLong");
        public static readonly FieldDecl Sport = Declarations.DeclareField(5, "Sport", true, false);
        public static readonly FieldDecl SubSport = Declarations.DeclareField(6, "SubSport", true, false);
        public static readonly FieldDecl TotalElapsedTime = Declarations.DeclareField(7, "TotalElapsedTime");
        public static readonly FieldDecl TotalTimerTime = Declarations.DeclareField(8, "TotalTimerTime");
        public static readonly FieldDecl TotalDistance = Declarations.DeclareField(9, "TotalDistance");
        public static readonly FieldDecl TotalCycles = Declarations.DeclareField(10, "TotalCycles");
        public static readonly FieldDecl TotalStrides = Declarations.DeclareField(10, "TotalStrides"); // alias
        public static readonly FieldDecl TotalCalories = Declarations.DeclareField(11, "TotalCalories");
        public static readonly FieldDecl TotalFatCalories = Declarations.DeclareField(13, "TotalFatCalories");
        public static readonly FieldDecl AvgSpeed = Declarations.DeclareField(14, "AvgSpeed");
        public static readonly FieldDecl MaxSpeed = Declarations.DeclareField(15, "MaxSpeed");
        public static readonly FieldDecl AvgHeartRate = Declarations.DeclareField(16, "AvgHeartRate");
        public static readonly FieldDecl MaxHeartRate = Declarations.DeclareField(17, "MaxHeartRate");
        public static readonly FieldDecl AvgCadence = Declarations.DeclareField(18, "AvgCadence");
        public static readonly FieldDecl AvgRunningCadence = Declarations.DeclareField(18, "AvgRunningCadence"); // alias
        public static readonly FieldDecl MaxCadence = Declarations.DeclareField(19, "MaxCadence");
        public static readonly FieldDecl MaxRunningCadence = Declarations.DeclareField(19, "MaxRunningCadence"); // alias
        public static readonly FieldDecl AvgPower = Declarations.DeclareField(20, "AvgPower");
        public static readonly FieldDecl MaxPower = Declarations.DeclareField(21, "MaxPower");
        public static readonly FieldDecl TotalAscent = Declarations.DeclareField(22, "TotalAscent");
        public static readonly FieldDecl TotalDescent = Declarations.DeclareField(23, "TotalDescent");
        public static readonly FieldDecl TotalTrainingEffect = Declarations.DeclareField(24, "TotalTrainingEffect");
        public static readonly FieldDecl FirstLapIndex = Declarations.DeclareField(25, "FirstLapIndex");
        public static readonly FieldDecl NumLaps = Declarations.DeclareField(26, "NumLaps");
        public static readonly FieldDecl EventGroup = Declarations.DeclareField(27, "EventGroup");
        public static readonly FieldDecl Trigger = Declarations.DeclareField(28, "Trigger", true, false);
        public static readonly FieldDecl NecLat = Declarations.DeclareField(29, "NecLat");
        public static readonly FieldDecl NecLong = Declarations.DeclareField(30, "NecLong");
        public static readonly FieldDecl SwcLat = Declarations.DeclareField(31, "SwcLat");
        public static readonly FieldDecl SwcLong = Declarations.DeclareField(32, "SwcLong");
        public static readonly FieldDecl NormalizedPower = Declarations.DeclareField(34, "NormalizedPower");
        public static readonly FieldDecl TrainingStressScore = Declarations.DeclareField(35, "TrainingStressScore");
        public static readonly FieldDecl IntensityFactor = Declarations.DeclareField(36, "IntensityFactor");
        public static readonly FieldDecl LeftRightBalance = Declarations.DeclareField(37, "LeftRightBalance");
        public static readonly FieldDecl AvgStrokeCount = Declarations.DeclareField(41, "AvgStrokeCount");
        public static readonly FieldDecl AvgStrokeDistance = Declarations.DeclareField(42, "AvgStrokeDistance");
        public static readonly FieldDecl SwimStroke = Declarations.DeclareField(43, "SwimStroke", true, false);
        public static readonly FieldDecl PoolLength = Declarations.DeclareField(44, "PoolLength");
        public static readonly FieldDecl PoolLengthUnit = Declarations.DeclareField(46, "PoolLengthUnit", true, false);
        public static readonly FieldDecl NumActiveLengths = Declarations.DeclareField(47, "NumActiveLengths");
        public static readonly FieldDecl TotalWork = Declarations.DeclareField(48, "TotalWork");
        public static readonly FieldDecl AvgAltitude = Declarations.DeclareField(49, "AvgAltitude");
        public static readonly FieldDecl MaxAltitude = Declarations.DeclareField(50, "MaxAltitude");
        public static readonly FieldDecl GpsAccuracy = Declarations.DeclareField(51, "GpsAccuracy");
        public static readonly FieldDecl AvgGrade = Declarations.DeclareField(52, "AvgGrade");
        public static readonly FieldDecl AvgPosGrade = Declarations.DeclareField(53, "AvgPosGrade");
        public static readonly FieldDecl AvgNegGrade = Declarations.DeclareField(54, "AvgNegGrade");
        public static readonly FieldDecl MaxPosGrade = Declarations.DeclareField(55, "MaxPosGrade");
        public static readonly FieldDecl MaxNegGrade = Declarations.DeclareField(56, "MaxNegGrade");
        public static readonly FieldDecl AvgTemperature = Declarations.DeclareField(57, "AvgTemperature");
        public static readonly FieldDecl MaxTemperature = Declarations.DeclareField(58, "MaxTemperature");
        public static readonly FieldDecl TotalMovingTime = Declarations.DeclareField(59, "TotalMovingTime");
        public static readonly FieldDecl AvgPosVerticalSpeed = Declarations.DeclareField(60, "AvgPosVerticalSpeed");
        public static readonly FieldDecl AvgNegVerticalSpeed = Declarations.DeclareField(61, "AvgNegVerticalSpeed");
        public static readonly FieldDecl MaxPosVerticalSpeed = Declarations.DeclareField(62, "MaxPosVerticalSpeed");
        public static readonly FieldDecl MaxNegVerticalSpeed = Declarations.DeclareField(63, "MaxNegVerticalSpeed");
        public static readonly FieldDecl MinHeartRate = Declarations.DeclareField(64, "MinHeartRate");
        public static readonly FieldDecl TimeInHrZone = Declarations.DeclareField(65, "TimeInHrZone", false, true);
        public static readonly FieldDecl TimeInSpeedZone = Declarations.DeclareField(66, "TimeInSpeedZone", false, true);
        public static readonly FieldDecl TimeInCadenceZone = Declarations.DeclareField(67, "TimeInCadenceZone", false, true);
        public static readonly FieldDecl TimeInPowerZone = Declarations.DeclareField(68, "TimeInPowerZone", false, true);
        public static readonly FieldDecl AvgLapTime = Declarations.DeclareField(69, "AvgLapTime");
        public static readonly FieldDecl BestLapIndex = Declarations.DeclareField(70, "BestLapIndex");
        public static readonly FieldDecl MinAltitude = Declarations.DeclareField(71, "MinAltitude");
        public static readonly FieldDecl PlayerScore = Declarations.DeclareField(82, "PlayerScore");
        public static readonly FieldDecl OpponentScore = Declarations.DeclareField(83, "OpponentScore");
        public static readonly FieldDecl OpponentName = Declarations.DeclareField(84, "OpponentName", false, true);  // this is a new type - byte array
        public static readonly FieldDecl StrokeCount = Declarations.DeclareField(85, "StrokeCount", false, true);
        public static readonly FieldDecl ZoneCount = Declarations.DeclareField(86, "ZoneCount", false, true);
        public static readonly FieldDecl MaxBallSpeed = Declarations.DeclareField(87, "MaxBallSpeed");
        public static readonly FieldDecl AvgBallSpeed = Declarations.DeclareField(88, "AvgBallSpeed");
        public static readonly FieldDecl AvgVerticalOscillation = Declarations.DeclareField(89, "AvgVerticalOscillation");
        public static readonly FieldDecl AvgStanceTimePercent = Declarations.DeclareField(90, "AvgStanceTimePercent");
        public static readonly FieldDecl AvgStanceTime = Declarations.DeclareField(91, "AvgStanceTime");
        public static readonly FieldDecl AvgFractionalCadence = Declarations.DeclareField(92, "AvgFractionalCadence");
        public static readonly FieldDecl MaxFractionalCadence = Declarations.DeclareField(93, "MaxFractionalCadence");
        public static readonly FieldDecl TotalFractionalCycles = Declarations.DeclareField(94, "TotalFractionalCycles");
        public static readonly FieldDecl AvgTotalHemoglobinConc = Declarations.DeclareField(95, "AvgTotalHemoglobinConc", false, true);
        public static readonly FieldDecl MinTotalHemoglobinConc = Declarations.DeclareField(96, "MinTotalHemoglobinConc", false, true);
        public static readonly FieldDecl MaxTotalHemoglobinConc = Declarations.DeclareField(97, "MaxTotalHemoglobinConc", false, true);
        public static readonly FieldDecl AvgSaturatedHemoglobinPercent = Declarations.DeclareField(98, "AvgSaturatedHemoglobinPercent", false, true);
        public static readonly FieldDecl MinSaturatedHemoglobinPercent = Declarations.DeclareField(99, "MinSaturatedHemoglobinPercent", false, true);
        public static readonly FieldDecl MaxSaturatedHemoglobinPercent = Declarations.DeclareField(100, "MaxSaturatedHemoglobinPercent", false, true);
        public static readonly FieldDecl AvgLeftTorqueEffectiveness = Declarations.DeclareField(101, "AvgLeftTorqueEffectiveness");
        public static readonly FieldDecl AvgRightTorqueEffectiveness = Declarations.DeclareField(102, "AvgRightTorqueEffectiveness");
        public static readonly FieldDecl AvgLeftPedalSmoothness = Declarations.DeclareField(103, "AvgLeftPedalSmoothness");
        public static readonly FieldDecl AvgRightPedalSmoothness = Declarations.DeclareField(104, "AvgRightPedalSmoothness");
        public static readonly FieldDecl AvgCombinedPedalSmoothness = Declarations.DeclareField(105, "AvgCombinedPedalSmoothness");
        public static readonly FieldDecl TimeStamp = Declarations.DeclareField(253, "TimeStamp");
        public static readonly FieldDecl MessageIndex = Declarations.DeclareField(254, "MessageIndex");
    }

    public enum Event : byte
    {
        Timer = 0,
        Workout = 3,
        WorkoutStep = 4,
        PowerDown = 5,
        PowerUp = 6,
        OffCourse = 7,
        Session = 8,
        Lap = 9,
        CoursePoint = 10,
        Battery = 11,
        VirtualPartnerPace = 12,
        HrHighAlert = 13,
        HrLowAlert = 14,
        SpeedHighAlert = 15,
        SpeedLowAlert = 16,
        CadHighAlert = 17,
        CadLowAlert = 18,
        PowerHighAlert = 19,
        PowerLowAlert = 20,
        RecoveryHr = 21,
        BatteryLow = 22,
        TimeDurationAlert = 23,
        DistanceDurationAlert = 24,
        CalorieDurationAlert = 25,
        Activity = 26,
        FitnessEquipment = 27,
        Length = 28,
        UserMarker = 32,
        SportPoint = 33,
        Calibration = 36,
        Invalid = 0xFF
    }

    public enum EventType : byte
    {
        Start = 0,
        Stop = 1,
        ConsecutiveDepreciated = 2,
        Marker = 3,
        StopAll = 4,
        BeginDepreciated = 5,
        EndDepreciated = 6,
        EndAllDepreciated = 7,
        StopDisable = 8,
        StopDisableAll = 9,
        Invalid = 0xFF
    }

    public class MessageDecl
    {
        public readonly ushort MessageNumber;
        public readonly string MessageName;
        public readonly FieldDecls FieldDeclarations;

        public MessageDecl(ushort messageNumber, string messageName, FieldDecls fieldDeclarations)
        {
            MessageNumber = messageNumber;
            MessageName = messageName;
            FieldDeclarations = fieldDeclarations;
        }

        public static implicit operator ushort(MessageDecl messageDecl)
        {
            return messageDecl.MessageNumber;
        }
    }

    // Global message numbers identify unique message types in the .FIT file format. AFAIK, all records
    // are dynamically defined within the .FIT file; that is the schema for each message type is encoded
    // within the file itself. These identifiers are only for well-known message types to help the parser
    // categorize the data itself.
    public static class GlobalMessageDecls
    {
        public static readonly Dictionary<int, MessageDecl> Declarations = new Dictionary<int, MessageDecl>();

        private static MessageDecl Add(byte messageNumber, string messageName, FieldDecls fieldDefinitions)
        {
            MessageDecl messageDef = new MessageDecl(messageNumber, messageName, fieldDefinitions);
            Declarations[messageNumber] = messageDef;
            return messageDef;
        }

        public static readonly MessageDecl FileId = Add(0, "FileId", FileIdDef.Declarations);
        public static readonly MessageDecl Capabilities = Add(1, "Capabilities", null);
        public static readonly MessageDecl DeviceSettings = Add(2, "DeviceSettings", null);
        public static readonly MessageDecl UserProfile = Add(3, "UserProfile", null);
        public static readonly MessageDecl HrmProfile = Add(4, "HrmProfile", null);
        public static readonly MessageDecl SdmProfile = Add(5, "SdmProfile", null);
        public static readonly MessageDecl BikeProfile = Add(6, "BikeProfile", null);
        public static readonly MessageDecl ZonesTarget = Add(7, "ZonesTarget", null);
        public static readonly MessageDecl HrZone = Add(8, "HrZone", null);
        public static readonly MessageDecl PowerZone = Add(9, "PowerZone", null);
        public static readonly MessageDecl MetZone = Add(10, "MetZone", null);
        public static readonly MessageDecl Sport = Add(12, "Sport", null);
        public static readonly MessageDecl Goal = Add(15, "Goal", null);
        public static readonly MessageDecl Session = Add(18, "Session", SessionDef.Declarations);
        public static readonly MessageDecl Lap = Add(19, "Lap", LapDef.Declarations);
        public static readonly MessageDecl Record = Add(20, "Record", RecordDef.Declarations);
        public static readonly MessageDecl Event = Add(21, "Event", EventDef.Declarations);
        public static readonly MessageDecl DeviceInfo = Add(23, "DeviceInfo", DeviceInfoDef.Declarations);
        public static readonly MessageDecl Workout = Add(26, "Workout", null);
        public static readonly MessageDecl WorkoutStep = Add(27, "WorkoutStep", null);
        public static readonly MessageDecl Schedule = Add(28, "Schedule", null);
        public static readonly MessageDecl WeightScale = Add(30, "WeightScale", null);
        public static readonly MessageDecl Course = Add(31, "Course", null);
        public static readonly MessageDecl CoursePoint = Add(32, "CoursePoint", null);
        public static readonly MessageDecl Totals = Add(33, "Totals", null);
        public static readonly MessageDecl Activity = Add(34, "Activity", ActivityDef.Declarations);
        public static readonly MessageDecl Software = Add(35, "Software", null);
        public static readonly MessageDecl FileCapabilities = Add(37, "FileCapabilities", null);
        public static readonly MessageDecl MesgCapabilities = Add(38, "MesgCapabilities", null);
        public static readonly MessageDecl FieldCapabilities = Add(39, "FieldCapabilities", null);
        public static readonly MessageDecl FileCreator = Add(49, "FileCreator", FileCreatorDef.Declarations);
        public static readonly MessageDecl BloodPressure = Add(51, "BloodPressure", null);
        public static readonly MessageDecl SpeedZone = Add(53, "SpeedZone", null);
        public static readonly MessageDecl Monitoring = Add(55, "Monitoring", null);
        public static readonly MessageDecl Hrv = Add(78, "Hrv", null);
        public static readonly MessageDecl Length = Add(101, "Length", null);
        public static readonly MessageDecl MonitoringInfo = Add(103, "MonitoringInfo", null);
        public static readonly MessageDecl Pad = Add(105, "Pad", null);
        public static readonly MessageDecl SlaveDevice = Add(106, "SlaveDevice", null);
        public static readonly MessageDecl CadenceZone = Add(131, "CadenceZone", null);
        public static readonly MessageDecl MemoGlob = Add(145, "MemoGlob", null);
    }
}