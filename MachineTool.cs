﻿/*
 * Copyright Copyright 2012, System Insights, Inc.
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AdapterLab
{
    using MTConnect;
    using NAudio;
    using NAudio.Wave;

    public partial class MachineTool : Form
    {
        Adapter mAdapter = new Adapter();
        Event mAvail = new Event("avail");
        Event mEStop = new Event("estop");

        Event mMode = new Event("mode");
        Event mExec = new Event("exec");

        Event mFunctionalMode = new Event("func");
        Event mProgram = new Event("program");
        Message mMessage = new Message("message");

        Sample mPosition = new Sample("xPosition");
        Sample mxLoad = new Sample("xLoad");

        Sample mSpeed = new Sample("sSpeed");
        Sample mcLoad = new Sample("sLoad");

        Condition mSystem = new Condition("system");
        Condition mTemp = new Condition("temp");
        Condition mOverload = new Condition("overload");
        Condition mTravel = new Condition("travel");
        Condition mFillLevel = new Condition("cool_low", true);

        public MachineTool()
        {
            InitializeComponent();
            stop.Enabled = false;

            mAdapter.AddDataItem(mAvail);
            mAvail.Value = "AVAILABLE";

            mAdapter.AddDataItem(mEStop);

            mAdapter.AddDataItem(mMode);
            mAdapter.AddDataItem(mExec);

            mAdapter.AddDataItem(mFunctionalMode);
            mAdapter.AddDataItem(mProgram);
            mAdapter.AddDataItem(mMessage);

            mAdapter.AddDataItem(mPosition);
            mAdapter.AddDataItem(mxLoad);

            mAdapter.AddDataItem(mSpeed);
            mAdapter.AddDataItem(mcLoad);

            mAdapter.AddDataItem(mSystem);
            mAdapter.AddDataItem(mTemp);
            mAdapter.AddDataItem(mOverload);
            mAdapter.AddDataItem(mTravel);
            mAdapter.AddDataItem(mFillLevel);
        }

        private void start_Click(object sender, EventArgs e)
        {
            // Start the adapter lib with the port number in the text box
            mAdapter.Port = Convert.ToInt32(port.Text);
            mAdapter.Start();

            // Disable start and enable stop.
            start.Enabled = false;
            stop.Enabled = true;

            // Start our periodic timer
            gather.Interval = 1000;
            gather.Enabled = true;

            mSystem.Normal();
            mTemp.Normal();
            mOverload.Normal();
            mTravel.Normal();
            mFillLevel.Normal();
        }

        private void stop_Click(object sender, EventArgs e)
        {
            // Stop everything...
            mAdapter.Stop();
            stop.Enabled = false;
            start.Enabled = true;
            gather.Enabled = false;
        }

        private void gather_Tick(object sender, EventArgs e)
        {
            mAdapter.Begin();

            if (estop.Checked)
                mEStop.Value = "TRIGGERED";
            else
                mEStop.Value = "ARMED";

            if (automatic.Checked)
                mMode.Value = "AUTOMATIC";
            else if (mdi.Checked)
                mMode.Value = "MANUAL_DATA_INPUT";
            else if (edit.Checked)
                mMode.Value = "EDIT";
            else
                mMode.Value = "MANUAL";

            if (running.Checked)
                mExec.Value = "ACTIVE";
            else if (feedhold.Checked)
                mExec.Value = "FEED_HOLD";
            else if (stopped.Checked)
                mExec.Value = "STOPPED";
            else if (ready.Checked)
                mExec.Value = "READY";

            mFunctionalMode.Value = functionalMode.Text;
            mProgram.Value = program.Text;

            if (messageCode.Text.Length > 0)
            {
                mMessage.Code = messageCode.Text;
                mMessage.Value = messageText.Text;
            }

            mxLoad.Value = xLoad.Value;
            mcLoad.Value = cLoad.Value;

            if (flazBat.Checked)
                mSystem.Add(Condition.Level.FAULT, "Yur Flaz Bat is flapping", "FLAZBAT");
            if (something.Checked)
                mSystem.Add(Condition.Level.WARNING, "Something went wrong", "AKAK");
            if (noProgram.Checked)
                mSystem.Add(Condition.Level.FAULT, "No program loaded", "PROG");

            if (overtemp.Checked)
                mTemp.Add(Condition.Level.WARNING, "Temperature is too high", "OT");
            if (overload.Checked)
                mOverload.Add(Condition.Level.FAULT, "Axis overload", "OL");
            if (travel.Checked)
                mTravel.Add(Condition.Level.FAULT, "Travel outside boundaries", "OP");

            mAdapter.SendChanged();
        }

        private void message_Leave(object sender, EventArgs e)
        {
            mMessage.Value = messageText.Text;
            mMessage.ForceChanged();
            mAdapter.SendChanged();
        }

        private void xLoad_Scroll(object sender, ScrollEventArgs e)
        {
            xLoadValue.Text = xLoad.Value.ToString();
        }

        private void xPosition_Scroll(object sender, ScrollEventArgs e)
        {
            mPosition.Value = xPosition.Value;
            mAdapter.SendChanged();

            xPositionValue.Text = xPosition.Value.ToString();
        }

        private void cLoad_Scroll(object sender, ScrollEventArgs e)
        {
            cLoadValue.Text = cLoad.Value.ToString();
        }

        private void cSpeed_Scroll(object sender, ScrollEventArgs e)
        {
            mSpeed.Value = cSpeed.Value * 100.0;
            mAdapter.SendChanged();

            cSpeedValue.Text = mSpeed.Value.ToString();
        }

        private void coolant_CheckedChanged(object sender, EventArgs e)
        {
            if (coolant.Checked)
                mFillLevel.Add(Condition.Level.WARNING, "Coolant Low", "COOL", "LOW");
            else
                mFillLevel.Clear("COOL");
            mAdapter.SendChanged();
        }

        private void cuttingToolButton_Click(object sender, EventArgs e)
        {
        }

        void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
        }
     }
}
