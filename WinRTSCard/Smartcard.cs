/**
 * @author Olivier ROUIT
 * 
 * @license CPL, CodeProject license 
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Core.WinRTSCard
{
    /// <summary>
    /// This is a simple component to manage a smartcard
    /// 
    /// Method are made synchronous because the service on the local machine and a smartcard
    /// although not very fast is a quite responsive device..
    /// </summary>
    public sealed class Smartcard
    {
        private SCardService.RemoteCardClient cardClient = null;
        private const int TIMEOUT = 10000;

        public Smartcard()
        {
            // Create an instance of the 
            cardClient = new SCardService.RemoteCardClient();
        }

        /// <summary>
        /// Gets the list of readers
        /// 
        /// REM: This method is not really at its place and should be in a seperate 
        /// component. Maybe later if I have some time
        /// </summary>
        /// <returns>A string array of the readers</returns>
        public string[] ListReaders()
        {
            Task<ObservableCollection<string>> readers = cardClient.ListReadersAsync();

            try
            {
                return readers.Result.ToArray();
            }
            catch (AggregateException ax)
            {
                throw new Exception(ProcessAggregateException(ax));
            }
        }

        /// <summary>
        /// Connects to a card. Establishes a card session
        /// </summary>
        /// <param name="Reader">Reader string</param>
        /// <param name="ShareMode">Session share mode</param>
        /// <param name="PreferredProtocols">Session preferred protocol</param>
        public void Connect(string reader, SHARE shareMode, PROTOCOL preferredProtocols)
        {
            try
            {
                cardClient.ConnectAsync(reader, (SCardService.SHARE)shareMode, (SCardService.PROTOCOL)preferredProtocols).Wait(TIMEOUT);
            }
            catch (AggregateException ax)
            {
                throw new Exception(ProcessAggregateException(ax));
            }
        }

        /// <summary> 
        /// Disconnect the current session
        /// </summary>
        /// <param name="Disposition">Action when disconnecting from the card</param>
        public void Disconnect(DISCONNECT disposition)
        {
            try
            {
                cardClient.DisconnectAsync((SCardService.DISCONNECT)disposition).Wait(TIMEOUT);
            }
            catch (AggregateException ax)
            {
                throw new Exception(ProcessAggregateException(ax));
            }
        }

        /// <summary>
        /// Transmit an APDU command to the card
        /// </summary>
        /// <param name="ApduCmd">APDU Command to send to the card</param>
        /// <returns>An APDU Response from the card</returns>
        public APDUResponse Transmit(APDUCommand apduCmd)
        {
            Task<SCardService.APDUResponse> task = cardClient.TransmitAsync(
                new SCardService.APDUCommand()
                {
                    Class = apduCmd.Class,
                    Ins = apduCmd.Ins,
                    P1 = apduCmd.P1,
                    P2 = apduCmd.P2,
                    Le = apduCmd.Le,
                    Data = apduCmd.Data
                });

            try
            {
                SCardService.APDUResponse resp = task.Result;

                return new APDUResponse()
                {
                    SW1 = resp.SW1,
                    SW2 = resp.SW2,
                    Data = resp.Data
                };
            }
            catch (AggregateException ax)
            {
                throw new Exception(ProcessAggregateException(ax));
            }
        }

        /// <summary>
        /// Begins a card transaction
        /// </summary>
        public void BeginTransaction()
        {
            try
            {
                cardClient.BeginTransactionAsync().Wait(TIMEOUT);
            }
            catch (AggregateException ax)
            {
                throw new Exception(ProcessAggregateException(ax));
            }
        }

        /// <summary>
        /// Ends a card transaction
        /// </summary>
        public void EndTransaction(DISCONNECT disposition)
        {
            try
            {
                cardClient.EndTransactionAsync((SCardService.DISCONNECT)disposition).Wait(TIMEOUT);
            }
            catch (AggregateException ax)
            {
                throw new Exception(ProcessAggregateException(ax));
            }
        }

        /// <summary>
        /// Gets the attributes of the card
        /// 
        /// This command can be used to get the Answer to reset
        /// </summary>
        /// <param name="AttribId">Identifier for the Attribute to get</param>
        /// <returns>Attribute content</returns>
        public byte[] GetAttribute(UInt32 attribId)
        {
            Task<byte[]> task = cardClient.GetAttributeAsync(attribId);

            try
            {
                return task.Result;
            }
            catch (AggregateException ax)
            {
                throw new Exception(ProcessAggregateException(ax));
            }
        }

        /// <summary>
        /// This method extract the error message carried by the WCF fault
        /// 
        /// Supports SmarcardFault and GeneralFault
        /// </summary>
        /// <param name="ax">AggregateException object</param>
        /// <returns>Extracted message</returns>
        private static string ProcessAggregateException(AggregateException ax)
        {
            string msg = "Unknown fault";
            FaultException<SCardService.SmartcardFault> scFault = ax.InnerException as FaultException<SCardService.SmartcardFault>;
            if (scFault != null)
            {
                msg = scFault.Detail.Message;
            }
            else
            {
                FaultException<SCardService.GeneralFault> exFault = ax.InnerException as FaultException<SCardService.GeneralFault>;
                if (exFault != null)
                {
                    msg = exFault.Detail.Message;
                }
            }

            return msg;
        }
	}

	/// <summary>
	/// SCOPE context
	/// </summary>
	public enum SCOPE
	{
		/// <summary>
		/// The context is a user context, and any database operations are performed within the
		/// domain of the user.
		/// </summary>
		User,		

		/// <summary>
		/// The context is that of the current terminal, and any database operations are performed
		/// within the domain of that terminal.  (The calling application must have appropriate
		/// access permissions for any database actions.)
		/// </summary>
		Terminal,	

		/// <summary>
		/// The context is the system context, and any database operations are performed within the
		/// domain of the system.  (The calling application must have appropriate access
		/// permissions for any database actions.)
		/// </summary>
		System	
	}

	/// <summary>
	/// SHARE mode enumeration
	/// </summary>
	public	enum SHARE
	{
		/// <summary>
		/// This application is not willing to share this card with other applications.
		/// </summary>
		Exclusive = 1,	

		/// <summary>
		/// This application is willing to share this card with other applications.
		/// </summary>
		Shared,			

		/// <summary>
		/// This application demands direct control of the reader, so it is not available to other applications.
		/// </summary>
		Direct			
	}

	/// <summary>
	/// PROTOCOL enumeration
	/// </summary>
	public	enum	PROTOCOL
	{
		/// <summary>
		/// There is no active protocol.
		/// </summary>
		Undefined	= 0x00000000,	

		/// <summary>
		/// T=0 is the active protocol.
		/// </summary>
		T0			= 0x00000001,	

		/// <summary>
		/// T=1 is the active protocol.
		/// </summary>
		T1			= 0x00000002,	

		/// <summary>
		/// Raw is the active protocol.
		/// </summary>
		Raw			= 0x00010000, 
		Default		= unchecked ((int) 0x80000000),  // Use implicit PTS.

		/// <summary>
		/// T=1 or T=0 can be the active protocol
		/// </summary>
		T0orT1		= T0 | T1
	}

	/// <summary>
	/// DISCONNECT action enumeration
	/// </summary>
	public	enum	DISCONNECT
	{
		/// <summary>
		/// Don't do anything special on close
		/// </summary>
		Leave,		

		/// <summary>
		/// Reset the card on close
		/// </summary>
		Reset,		

		/// <summary>
		/// Power down the card on close
		/// </summary>
		Unpower,	

		/// <summary>
		/// Eject(!) the card on close
		/// </summary>
		Eject	
	}
}
