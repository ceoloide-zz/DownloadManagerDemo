using System.Windows.Data;
using System;
using System.Globalization;
using System.Windows;
using TransferManager;
using DownloadManager.Resources;

namespace DownloadManager.Converters
{
    public class TransferStatusToStartVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                ExtendedTransferStatus TransferStatus = (ExtendedTransferStatus)value; ;
                return (TransferStatus == ExtendedTransferStatus.Paused
                    || TransferStatus == ExtendedTransferStatus.Queued
                    || TransferStatus == ExtendedTransferStatus.Transferring
                    || TransferStatus == ExtendedTransferStatus.Waiting
                    || TransferStatus == ExtendedTransferStatus.WaitingForRetry
                    || TransferStatus == ExtendedTransferStatus.WaitingForExternalPower
                    || TransferStatus == ExtendedTransferStatus.WaitingForExternalPowerDueToBatterySaverMode
                    || TransferStatus == ExtendedTransferStatus.WaitingForNonVoiceBlockingNetwork
                    || TransferStatus == ExtendedTransferStatus.WaitingForWiFi
                    || TransferStatus == ExtendedTransferStatus.Failed)
                    ? Visibility.Collapsed : Visibility.Visible;
            }
            catch (Exception)
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }

    public class TransferStatusToStopVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                ExtendedTransferStatus TransferStatus = (ExtendedTransferStatus)value; ;
                return (TransferStatus == ExtendedTransferStatus.Paused
                    || TransferStatus == ExtendedTransferStatus.Queued
                    || TransferStatus == ExtendedTransferStatus.Transferring
                    || TransferStatus == ExtendedTransferStatus.Waiting
                    || TransferStatus == ExtendedTransferStatus.WaitingForRetry
                    || TransferStatus == ExtendedTransferStatus.WaitingForExternalPower
                    || TransferStatus == ExtendedTransferStatus.WaitingForExternalPowerDueToBatterySaverMode
                    || TransferStatus == ExtendedTransferStatus.WaitingForNonVoiceBlockingNetwork
                    || TransferStatus == ExtendedTransferStatus.WaitingForWiFi)
                    ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception)
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }

    public class TransferStatusToPercentageVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                ExtendedTransferStatus TransferStatus = (ExtendedTransferStatus)value; ;
                return (TransferStatus == ExtendedTransferStatus.Transferring) ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception)
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }

    public class IndeterminateTransferToBackgroundBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool IsIndetermiante = (bool)value;
                return (IsIndetermiante) ? Application.Current.Resources["PhoneAccentBrush"] : Application.Current.Resources["PhoneContrastBackgroundBrush"];
            }
            catch (Exception)
            {
                return Application.Current.Resources["PhoneAccentBrush"];
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }

    public class TransferStatusToStartText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                ExtendedTransferStatus TransferStatus = (ExtendedTransferStatus)value;
                return (TransferStatus == ExtendedTransferStatus.Canceled
                    || TransferStatus == ExtendedTransferStatus.Completed
                    || TransferStatus == ExtendedTransferStatus.FailedServer)
                    ? AppResources.Common_retry : AppResources.Common_start;
            }
            catch (Exception)
            {
                return AppResources.Common_start;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }

    public class DoubleToPercentageString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d = ((double)value);
            return (d == 0) ? "" : String.Format("{0:0}", d*100) + "%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }

    public class TransferStatusToProgressText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ExtendedTransferStatus)value)
            {
                case ExtendedTransferStatus.None:
                    return AppResources.ExtendedStatusToText_None;
                case ExtendedTransferStatus.Queued:
                    return AppResources.ExtendedStatusToText_Queued;
                case ExtendedTransferStatus.Transferring:
                    return AppResources.ExtendedStatusToText_Transferring;
                case ExtendedTransferStatus.Canceled:
                    return AppResources.ExtendedStatusToText_Canceled;
                case ExtendedTransferStatus.Waiting:
                    return AppResources.ExtendedStatusToText_Waiting;
                case ExtendedTransferStatus.WaitingForNonVoiceBlockingNetwork:
                    return AppResources.ExtendedStatusToText_WaitingForNonVoiceBlockingNetwork;
                case ExtendedTransferStatus.Paused:
                    return AppResources.ExtendedStatusToText_Paused;
                case ExtendedTransferStatus.Completed:
                    return AppResources.ExtendedStatusToText_Completed;
                case ExtendedTransferStatus.WaitingForRetry:
                    return AppResources.TapMessage_WaitingForRetry;
                case ExtendedTransferStatus.WaitingForWiFi:
                    return AppResources.ExtendedStatusToText_WaitingForWiFi;
                case ExtendedTransferStatus.WaitingForExternalPower:
                    return AppResources.ExtendedStatusToText_WaitingForExternalPower;
                case ExtendedTransferStatus.WaitingForExternalPowerDueToBatterySaverMode:
                    return AppResources.ExtendedStatusToText_WaitingForExternalPowerDueToBatterySaverMode;
                case ExtendedTransferStatus.Failed:
                    return AppResources.ExtendedStatusToText_Failed;
                case ExtendedTransferStatus.FailedServer:
                    return AppResources.ExtendedStatusToText_FailedServer;
                default:
                   return AppResources.ExtendedStatusToText_Null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }
}
