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

            ClassLength = 2;
            InsLength = 2;
            DataLength = 4;
            P1Length = 2;
            P2Length = 2;

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
                return Core.Util.Convert.HexToString(apduCommand.Class); 
            }

            set
            {
                ClassLength = value.Length;
                apduCommand.Class = Core.Util.Convert.ParseByteToHex(value);
                RaisePropertyChanged(ApduCmdProp.Class);
                TransmitCommand.RaiseCanExecuteChanged();
            }
        }

        public int ClassLength
        {
            get;
            private set;
        }

        public string Ins
        {
            get 
            { 
                return Core.Util.Convert.HexToString(apduCommand.Ins);
            }

            set
            {
                InsLength = value.Length;
                apduCommand.Ins = Core.Util.Convert.ParseByteToHex(value);
                RaisePropertyChanged(ApduCmdProp.Ins);
                TransmitCommand.RaiseCanExecuteChanged();
            }
        }

        public int InsLength
        {
            get;
            private set;
        }

        public string P1
        {
            get 
            {
                return Core.Util.Convert.HexToString(apduCommand.P1);
            }

            set
            {
                P1Length = value.Length;
                apduCommand.P1 = Core.Util.Convert.ParseByteToHex(value);
                RaisePropertyChanged(ApduCmdProp.P1);
                TransmitCommand.RaiseCanExecuteChanged();
            }
        }

        public int P1Length
        {
            get;
            private set;
        }

        public string P2
        {
            get { return Core.Util.Convert.HexToString(apduCommand.P2); }
            set
            {
                apduCommand.P2 = Core.Util.Convert.ParseByteToHex(value);
                RaisePropertyChanged(ApduCmdProp.P2);
                TransmitCommand.RaiseCanExecuteChanged();
            }
        }

        public int P2Length
        {
            get;
            private set;
        }

        public string Le
        {
            get { return apduCommand.Le.ToString(); }
            set
            {
                P2Length = value.Length;
                apduCommand.Le = byte.Parse(value);
                RaisePropertyChanged(ApduCmdProp.Le);
                TransmitCommand.RaiseCanExecuteChanged();
            }
        }

        public string Data
        {
            get { return Core.Util.Convert.BufferToString(apduCommand.Data); }
            set
            {
                DataLength = value.Length;
                apduCommand.Data = Core.Util.Convert.ParseBufferToHex(value);
                RaisePropertyChanged(ApduCmdProp.Data);
                TransmitCommand.RaiseCanExecuteChanged();
            }
        }

        public int DataLength
        {
            get;
            private set;
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
                ClassLength == 2 && InsLength == 2 &&
                P1Length == 2 && P2Length == 2 &&
                (DataLength % 2) == 0;
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
    }
}
