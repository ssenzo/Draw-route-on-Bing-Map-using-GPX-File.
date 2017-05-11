/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 2017-05-10
 * Time: 오후 2:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

using Microsoft.Maps.MapControl.WPF;
using System.IO;
using System.Xml;
using System.Net;
using System.Web;
using System.Runtime.Serialization;

namespace BingMap02
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{

        string windowTitle;
        
		public Window1()
		{
			InitializeComponent();
		}
		void window1_Loaded(object sender, RoutedEventArgs e)
		{
			myMap.ZoomLevel = 16;
			var startLocation = new Location(37.5461013163009, 127.334279338546);
			myMap.Center = startLocation;
            this.windowTitle = window1.Title;
		}
		
		void rdRoad_Checked(object sender, RoutedEventArgs e)
		{
			if (myMap != null) {
				myMap.Mode = new RoadMode();
			}
		}
		void rdSattelite_Checked(object sender, RoutedEventArgs e)
		{
			if (myMap != null) {
				myMap.Mode = new AerialMode(true);
			}
		}
		void dgFileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if ( ((DataGrid)sender).SelectedIndex >= 0) {
				var sFileName = ((DataGrid)sender).SelectedItems[0].ToString();
				
				var xmldoc = new XmlDocument(); //XmlDataDocument();
	            XmlNodeList xmlnode ;
	            int i = 0;
	            var fs = new FileStream(lblFolder.Content + @"\" + sFileName, FileMode.Open, FileAccess.Read);
	            xmldoc.Load(fs);
	            xmlnode = xmldoc.GetElementsByTagName("trkpt");
	            myMap.Children.Clear();
	            var myPolyline = new MapPolyline();
	            myPolyline.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Blue);
                myPolyline.StrokeThickness = 5;
                myPolyline.Locations = new LocationCollection();
                var startLocation = new Location();
                var lastLocation = new Location();
                double minLat = 0; 
                double minLon = 0;
                double maxLat = 0;
				double maxLon = 0;
                
	            for (i = 0; i <= xmlnode.Count - 1; i++)
	            {
	            	double lat = Convert.ToDouble(xmlnode[i].Attributes["lat"].Value.ToString());
	            	double lon = Convert.ToDouble(xmlnode[i].Attributes["lon"].Value.ToString());
	            	myPolyline.Locations.Add(new Microsoft.Maps.MapControl.WPF.Location(lat, lon));
	            	
	            	if (i==0) startLocation = new Location(lat, lon);
	            	if (i==(xmlnode.Count -1)) lastLocation = new Location(lat, lon);
	            	
	            	if (i==0) {
	            		minLat = lat;
	            		minLon = lon;
	            		maxLat = lat;
	            		maxLon = lon;
	            	}
	            	else {
	            		if (lat < minLat) minLat = lat;
	            		if (lon < minLon) minLon = lon;
	            		if (lat > maxLat) maxLat = lat;
	            		if (lon > maxLon) maxLon = lon;
	            	}
	            }
				
	            myMap.Children.Add(myPolyline);
	            
	            var locationRectangle = new List<Location>();
	            locationRectangle.Add(new Location(minLat, minLon));
	            locationRectangle.Add(new Location(maxLat, maxLon));
	            
				var boundingBox = new LocationRect(locationRectangle);	

				myMap.SetView(boundingBox);
				myMap.ZoomLevel = myMap.ZoomLevel - 0.2;
				
				var startPin = new Pushpin();
				var endPin = new Pushpin();
				startPin.Location = startLocation;
				endPin.Location = lastLocation;
				
				myMap.Children.Add(startPin);
				myMap.Children.Add(endPin);
			}
		}
		void btnSelectDir_Click(object sender, RoutedEventArgs e)
		{
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".*";
            dlg.Filter = "GPX Files (*.gpx)|*.gpx";
 
            var result = dlg.ShowDialog();
            if (result == true)
            {
                lblFolder.Content = dlg.FileName;
                string folderName = "";
                string[] strTemp = dlg.FileName.Split('\\');
                for (int i=0; i<(strTemp.Length-1); i++)
                {
                	if (folderName == "")
                	{
                		folderName = strTemp[i];
                	}
                	else 
                	{
                		folderName = folderName + @"\" + strTemp[i];
                	}
                }
                lblFolder.Content = folderName;
                
                var dirInfo = new DirectoryInfo(folderName);
				FileInfo[] info = dirInfo.GetFiles("*.gpx"); //dirInfo.GetFiles("*.*", SearchOption.AllDirectories);
			
				dgFileList.ItemsSource = info;
				dgFileList.SelectionMode = DataGridSelectionMode.Single;
				
				for (int i = 0; i < dgFileList.Items.Count; i++)
				{
					if (dgFileList.Items[i].ToString() == dlg.FileName.Replace(folderName + @"\", ""))
					{
						dgFileList.SelectedItem = dgFileList.Items[i];
					}
				}
            }
		}
		
		void myMap_MouseMove(object sender, MouseEventArgs e)
		{
			Point mousePosition = e.GetPosition(this);
        	Location pinLocation = myMap.ViewportPointToLocation(mousePosition);
            window1.Title = windowTitle + "    Lat:" + pinLocation.Latitude.ToString() + " Lon:" + pinLocation.Longitude.ToString();
		}
		
		
		
		void btnTest_Click(object sender, RoutedEventArgs e)
		{
			string searchLocation = txtLocationToSearch.Text;
			string baseUrl = "http://dev.virtualearth.net/REST/v1/Locations?q=" 
				+ searchLocation 
				+ "&o=xml&key=AjidenPWkoOfH5pFY4nYxyNfYS-21JZrUOh7xvIRfIRaPLS3xVgiBbzL0qaTkE3P";
			//locationQuery?includeNeighborhood=includeNeighborhood&maxResults=maxResults&include=includeValue&key=BingMapsKey"
			
            var myUri = new Uri(baseUrl);
			var request = (HttpWebRequest)WebRequest.Create(myUri);
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            /*
            Stream stream = request.GetResponse().GetResponseStream();
            XmlReader reader = XmlReader.Create(stream);
            var xmldoc = new XmlDocument();
            xmldoc.Load(stream);
            */

            Stream resStream;
            
            resStream = response.GetResponseStream();
			string tempString = null;
			var sb = new StringBuilder();
			var buf = new byte[8192];
			int count = 0;
			do
			{
				count = resStream.Read(buf, 0, buf.Length);
				if (count != 0)
				{
					tempString = Encoding.UTF8.GetString(buf, 0, count);
					sb.Append(tempString);
				}
			}
			while (count > 0);
            
            var xmldoc = new XmlDocument();
            string tempStr;
            tempStr = sb.ToString();
            while (true)
            {
                if (tempStr.Substring(0, 1) == "<") break;

                tempStr = tempStr.Substring(1);
            }
            xmldoc.LoadXml(tempStr);
            
            var xmlnode = xmldoc.GetElementsByTagName("Point");

            if (xmlnode.Count > 0)
            {
                double lat = Convert.ToDouble(xmlnode[0]["Latitude"].InnerText);
                double lon = Convert.ToDouble(xmlnode[0]["Longitude"].InnerText);
                var loc = new Location(lat, lon);
                myMap.Center = loc;

                Pushpin pin = new Pushpin();
                pin.Location = loc;
                myMap.Children.Add(pin);
            }
            else {
                MessageBox.Show("[" + txtLocationToSearch.Text + "] 찾을 수 없습니다.");
            }
		}

        private void txtLocationToSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnTest_Click(sender, e);
            }
        }
    }
}