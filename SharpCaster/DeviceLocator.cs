using Rssdp;
using SharpCaster.Annotations;
using SharpCaster.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SharpCaster
{
    public class DeviceLocator : INotifyPropertyChanged
    {
        private const string CHROMECAST_DEVICE_TYPE = "urn:dial-multiscreen-org:device:dial:1";

        public ObservableCollection<Chromecast> DiscoveredDevices { get; set; }

        public DeviceLocator()
        {
            DiscoveredDevices = new ObservableCollection<Chromecast>();
        }

        public async Task<ObservableCollection<Chromecast>> LocateDevicesAsync()
        {
            using (var deviceLocator = new SsdpDeviceLocator())
            {
                var foundDevices = await deviceLocator.SearchAsync(CHROMECAST_DEVICE_TYPE, TimeSpan.FromMilliseconds(5000));

                foreach (var foundDevice in foundDevices)
                {
                    // Make sure is a Chromecast device
                    if (foundDevice.NotificationType != CHROMECAST_DEVICE_TYPE)
                    {
                        continue;
                    }
                    var fullDevice = await foundDevice.GetDeviceInfo();
                    Uri myUri;
                    Uri.TryCreate("https://" + foundDevice.DescriptionLocation.Host, UriKind.Absolute, out myUri);
                    var chromecast = new Chromecast
                    {
                        DeviceUri = myUri,
                        FriendlyName = fullDevice.FriendlyName
                    };
                    DiscoveredDevices.Add(chromecast);
                }
            }
            return DiscoveredDevices;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}