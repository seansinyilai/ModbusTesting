using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusConnection
{
    public enum FunctionCode
    {
        ReadCoils = 1,
        ReadDiscreteInputs = 2,
        ReadHoldingRegisters = 3,
       // ReadInputRegisters = 4,
        WriteSingleCoil = 5,
        WriteSingleRegister = 6,
        WriteMultipleCoils = 0x0F,
        WriteMultipleRegisters = 0x10
    }
    public enum ErrorCode
    {
        ReadCoilsError = 0x81,
        ReadDiscreteInputsError = 0x82,
        ReadHoldingRegistersError = 0x83,
      //  ReadInputRegistersError = 0x84,
        WriteSingleCoilError = 0x85,
        WriteSingleRegisterError = 0x86,
        WriteMultipleCoilsError = 0x8F,
        WriteMultipleRegistersError = 0x90
    }
    public enum ExceptionCode
    {
        IllegalFunction = 0x01,
        IllegalDataAddress = 0x02,
        IllegalDataValue = 0x03,
        IllegalServerDeviceFailure = 0x04,
        Acknowledge = 0x05,
        ServerDeviceBusy = 0x06,
        MemoryParityError = 0x08,
        GateWayPathUnavailable = 0x0A,
        GateWayTargetDeviceFailedToRespond = 0x0B,
    }
    public enum Action
    {
        ToRead,
        ToWrite
    }
}
