﻿using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Shiny;

[assembly: ShinyApplication(
    ShinyStartupTypeName = "ShinyBluetoothTest.Startup",
    XamarinFormsAppTypeName = "ShinyBluetoothTest.App"
)]

namespace ShinyBluetoothTest.Droid
{
    public partial class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        //protected override void OnCreate(Bundle savedInstanceState)
        //{
        //    base.OnCreate(savedInstanceState);

        //    Xamarin.Essentials.Platform.Init(this, savedInstanceState);
        //    global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
        //    LoadApplication(new App());
        //}
        //public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        //{
        //    Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        //    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        //}
    }
}
