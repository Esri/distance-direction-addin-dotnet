﻿// Copyright 2016 Esri 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Windows.Controls;

namespace DistanceAndDirectionLibrary.Views
{
    /// <summary>
    /// Interaction logic for ProCircleView.xaml
    /// </summary>
    public partial class ProCircleView : UserControl
    {
        public ProCircleView()
        {
            InitializeComponent();
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            DistanceAndDirectionLibrary.Helpers.Mediator.NotifyColleagues(DistanceAndDirectionLibrary.Constants.POINT_TEXT_KEYDOWN, null);
        }

        private void Radius_Diameter_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            DistanceAndDirectionLibrary.Helpers.Mediator.NotifyColleagues(DistanceAndDirectionLibrary.Constants.RADIUS_DIAMETER_KEYDOWN, null);
        }
    }
}