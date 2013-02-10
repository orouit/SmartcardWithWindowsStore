/**
 * @author Olivier ROUIT
 * 
 * @license CPL, CodeProject license 
 */

using Core.Util;
using Core.WinRTSCard;
using Core.Mvvm;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SCardDemoApp
{
    class APDUExchangeViewModel : BaseViewModel<APDUExchangeViewModel>
    {
        delegate void SetPropertyDel(string inData);
        delegate bool ConditionDel(string data);


        class ApduCmdProp
        {
            public const string
                Class = "Class",
                Ins = "Ins",
                P1 = "P1",
                P2 = "P2",
                Le = "Le",
                Data = "Data";
        }

        class ApduRespProp
        {
            public const string
                SW1 = "SW1",
                SW2 = "SW2",
                ResponseData = "ResponseData";
        }

        const string 
            ReaderNameProp = "ReaderName",
            ConnectedProp = "Connected",
            StatusProp = "Status";

        #region Fields

        private string sClass = string.Empty;
        private string sIns = string.Empty;
        private string sP1 = string.Empty;
        private string sP2 = string.Empty;
        private string sData = string.Empty;

        private Smartcard card = null;
        private APDUCommand apduCommand = null;
        private APDUResponse apduResponse = null;
        private string readerName = string.Empty;
        private bool connected = false;
        private string status;

        #endregion

        public APDUExchangeViewModel(Smartcard card)
        {
            this.card = card;
            apduCommand = new APDUCommand();
            apduCommand.Class = 0xA0;
            apduCommand.Ins = 0xA4;
            apduCommand.Data = new byte[] { 0x3F, 0x00 };

            sClass = "A0";
            sIns = "A4";
            sP1 = "00";
            sP2 = "00";
            sData = "3F00";

            CreateConnectCommand();
            CreateDisconnectCommand();
            CreateTransmitCommand();
        }

        #region Internal Properties 

        public bool Connected
        {
            get { return connected; }
            set
            {
                connected = value;
                RaisePropertyChanged(ConnectedProp);
                ConnectCommand.RaiseCanExecuteChanged();
                DisconnectCommand.RaiseCanExecuteChanged();
                TransmitCommand.RaiseCanExecuteChanged();
            }
        }

        public string[] ReaderList
        {
            get 
            { 
                return card.ListReaders(); 
            }
        }

        public string ReaderName
        {
            get 
            { 
                return readerName; 
            }

            set
            {
                readerName = value;
                RaisePropertyChanged(ReaderNameProp);
                ConnectCommand.RaiseCanExecuteChanged();
                DisconnectCommand.RaiseCanExecuteChanged();
            }
        }
        
        #endregion

        #region APDUCommand view model 

        public string Class
        {
            get 
            { 
                return sClass;
            }

            set
            {
                PropertySet(ref sClass, delegate(string inDt)
                {
                    apduCommand.Class = Core.Util.Convert.ParseByteToHex(value);
                }, ApduCmdProp.Class, value, ByteCondition);
            }
        }

        public string Ins
        {
            get 
            { 
                return sIns;
            }

            set
            {
                PropertySet(ref sIns, delegate(string inDt)
                {
                    apduCommand.Ins = Core.Util.Convert.ParseByteToHex(value);
                }, ApduCmdProp.Ins, value, ByteCondition);
            }
        }

        public string P1
        {
            get 
            {
                return sP1;
            }

            set
            {
                PropertySet(ref sP1, delegate(string inDt)
                {
                    apduCommand.P1 = Core.Util.Convert.ParseByteToHex(value);
                }, ApduCmdProp.P1, value, ByteCondition);
            }
        }

        public string P2
        {
            get 
            { 
                return sP2;
            }

            set
            {
                PropertySet(ref sP2, delegate(string inDt)
                {
                    apduCommand.P2 = Core.Util.Convert.ParseByteToHex(value);
                }, ApduCmdProp.P2, value, ByteCondition);
            }
        }

        public string Le
        {
            get { return apduCommand.Le.ToString(); }
            set
            {
                apduCommand.Le = byte.Parse(value);
                RaisePropertyChanged(ApduCmdProp.Le);
                TransmitCommand.RaiseCanExecuteChanged();
            }
        }

        public string Data
        {
            get 
            { 
                return sData;
            }

            set
            {
                PropertySet(ref sData, delegate(string inDt)
                {
                    apduCommand.Data = Core.Util.Convert.ParseBufferToHex(value);
                }, ApduCmdProp.Data, value, ByteCondition);
            }
        }
        
        #endregion

        #region APDUResponse view model

        public string SW1
        {
            get
            {
                return apduResponse != null ? Core.Util.Convert.HexToString(apduResponse.SW1) : string.Empty;
            }
        }

        public string SW2
        {
            get
            {
                return apduResponse != null ? Core.Util.Convert.HexToString(apduResponse.SW2) : string.Empty;
            }
        }

        public string ResponseData
        {
            get
            {
                return apduResponse != null ? Core.Util.Convert.BufferToString(apduResponse.Data) : string.Empty; 
            }
        }

        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                RaisePropertyChanged(StatusProp);
            }
        }

        #endregion

        #region Commands

        public RelayCommand ConnectCommand
        {
            get;
            internal set;
        }

        private bool CanExecuteConnectCommand()
        {
            return !Connected && !string.IsNullOrEmpty(ReaderName);
        }

        private void CreateConnectCommand()
        {
            ConnectCommand = new RelayCommand(ConnectExecute, CanExecuteConnectCommand);
        }

        public void ConnectExecute()
        {
            try
            {
                card.Connect(ReaderName, SHARE.Shared, PROTOCOL.T0orT1);
                Status = "Card connected.";
                Connected = true;
            }
            catch (Exception ex)
            {
                Status = ex.Message;
                Connected = false;
            }
        }

        public RelayCommand DisconnectCommand
        {
            get;
            internal set;
        }

        private bool CanExecuteDisconnectCommand()
        {
            return Connected;
        }

        private void CreateDisconnectCommand()
        {
            DisconnectCommand = new RelayCommand(DisconnectExecute, CanExecuteDisconnectCommand);
        }

        public void DisconnectExecute()
        {
            try
            {
                card.Disconnect(DISCONNECT.Unpower);
                Status = "Card disconnected.";

                Connected = false;
            }
            catch (Exception ex)
            {
                Status = ex.Message;
                Connected = false;
            }
        }

        public RelayCommand TransmitCommand
        {
            get;
            internal set;
        }

        private bool CanExecuteTransmitCommand()
        {
            return Connected && 
                (Class.Length == 2 && HexHelper.AreValidHexDigits(Class)) && 
                (Ins.Length == 2 && HexHelper.AreValidHexDigits(Ins)) &&
                (P1.Length == 2 && HexHelper.AreValidHexDigits(P1)) && 
                (P2.Length == 2 && HexHelper.AreValidHexDigits(P2)) &&
                (Data.Length == 0 || ((Data.Length % 2) == 0 && HexHelper.AreValidHexDigits(Data)));
        }

        private void CreateTransmitCommand()
        {
            TransmitCommand = new RelayCommand(TransmitExecute, CanExecuteTransmitCommand);
        }

        public void TransmitExecute()
        {
            try
            {
                apduResponse = card.Transmit(apduCommand);

                Status = "Transmit successful.";
                RaisePropertyChanged(ApduRespProp.SW1);
                RaisePropertyChanged(ApduRespProp.SW2);
                RaisePropertyChanged(ApduRespProp.ResponseData);
            }
            catch (Exception ex)
            {
                Status = ex.Message + ". Please disconnect and reconnect to continue";
            }

        }

        #endregion

        private bool ByteCondition(string inData)
        {
            return inData.Length == 2;
        }

        private bool BufferCondition(string inData)
        {
            return inData.Length % 2 == 0;
        }

        private void PropertySet(ref string field, SetPropertyDel setProperty, string propName, string inData, ConditionDel condition)
        {
            field = inData;
            if (condition(inData))
            {
                if (HexHelper.AreValidHexDigits(inData))
                {
                    setProperty(inData);
                    RaisePropertyChanged(propName);
                }
            }
            TransmitCommand.RaiseCanExecuteChanged();
        }
    }
}
