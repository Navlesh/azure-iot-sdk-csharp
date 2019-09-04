﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Azure.Iot.DigitalTwin.Device;
using Azure.Iot.DigitalTwin.Device.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace EnvironmentalSensorSample
{
    /// <summary>
    /// sample for DigitalTwinInterfaceClient implementation.
    /// </summary>
    public class EnvironmentalSensorInterface : DigitalTwinInterfaceClient
    {
        private const string EnvironmentalSensorInterfaceId = "urn:csharp_sdk_sample:EnvironmentalSensor:1";
        private const string DeviceState = "state";
        private const string CustomerName = "name";
        private const string Brightness = "brightness";
        private const string Temperature = "temp";
        private const string Humidity = "humid";
        private const string BlinkCommandName = "blink";
        private const string TurnOnLightCommad = "turnon";
        private const string TurnOffLightCommand = "turnoff";

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentalSensorInterface"/> class.
        /// </summary>
        /// <param name="interfaceName">interface name.</param>
        public EnvironmentalSensorInterface(string interfaceName)
            : base(EnvironmentalSensorInterfaceId, interfaceName, true, true)
            {
        }

        /// <summary>
        /// Process CustomerName property updated.
        /// </summary>
        /// <param name="customerNameUpdate">information of property to be reported.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SetCustomerNameAsync(DigitalTwinPropertyUpdate customerNameUpdate)
        {
            // code to consume customer value, currently just displaying on screen.
            string customerName = customerNameUpdate.PropertyDesired;
            Console.WriteLine($"Desired customer name = '{customerName}'.");
            Console.WriteLine($"Reported customer name = '{customerNameUpdate.PropertyReported}'.");
            Console.WriteLine($"Version is '{customerNameUpdate.DesiredVersion}'.");

            // report Completed
            var propertyReport = new Collection<DigitalTwinPropertyReport>();
            propertyReport.Add(new DigitalTwinPropertyReport(
                customerNameUpdate.PropertyName,
                customerNameUpdate.PropertyDesired,
                new DigitalTwinPropertyResponse(customerNameUpdate.DesiredVersion, 200, "Processing Completed")));
            await this.ReportPropertiesAsync(propertyReport).ConfigureAwait(false);
            Console.WriteLine("Sent completed status.");
        }

        /// <summary>
        /// Process Brightness property updated.
        /// </summary>
        /// <param name="brightnessUpdate">information of property to be reported.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SetBrightnessAsync(DigitalTwinPropertyUpdate brightnessUpdate)
        {
            // code to consume light brightness value, currently just displaying on screen
            string brightness = brightnessUpdate.PropertyDesired;
            long current = 0;

            Console.WriteLine($"Desired brightness = '{brightness}'.");
            Console.WriteLine($"Reported brightness = '{brightnessUpdate.PropertyReported}'.");
            Console.WriteLine($"Version is '{brightnessUpdate.DesiredVersion}'.");

            // report Pending
            var propertyReport = new Collection<DigitalTwinPropertyReport>();
            propertyReport.Add(new DigitalTwinPropertyReport(
                brightnessUpdate.PropertyName,
                current.ToString(),
                new DigitalTwinPropertyResponse(brightnessUpdate.DesiredVersion, 102, "Processing Request")));
            await this.ReportPropertiesAsync(propertyReport).ConfigureAwait(false);
            Console.WriteLine("Sent pending status for brightness property.");
            propertyReport.Clear();

            // Pretend calling command to Sensor to update brightness
            await Task.Delay(5 * 1000).ConfigureAwait(false);

            // report Completed
            propertyReport.Add(new DigitalTwinPropertyReport(
                brightnessUpdate.PropertyName,
                brightnessUpdate.PropertyDesired,
                new DigitalTwinPropertyResponse(
                    brightnessUpdate.DesiredVersion,
                    200,
                    "Request completed")));
            await this.ReportPropertiesAsync(propertyReport).ConfigureAwait(false);
            Console.WriteLine("Sent completed status for brightness property.");
        }

        /// <summary>
        /// sample for reporting a property on an interface.
        /// </summary>
        /// <param name="state">state property.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeviceStatePropertyAsync(bool state)
        {
            var deviceStateProperty = new DigitalTwinPropertyReport(DeviceState, state.ToString().ToLower());
            await this.ReportPropertiesAsync(new Collection<DigitalTwinPropertyReport> { deviceStateProperty }).ConfigureAwait(false);
        }

        /// <summary>
        /// Send Temperature telemetry.
        /// </summary>
        /// <param name="temperature">telemetry value.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendTemperatureAsync(double temperature)
        {
            await this.SendTelemetryAsync(Temperature, temperature.ToString()).ConfigureAwait(false);
        }

        /// <summary>
        /// Send Humidity telemetry.
        /// </summary>
        /// <param name="humidity">telemetry value.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendHumidityAsync(double humidity)
        {
            await this.SendTelemetryAsync(Humidity, humidity.ToString()).ConfigureAwait(false);
        }


        /// <summary>
        /// Callback on command received.
        /// </summary>
        /// <param name="commandRequest">information regarding the command received.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected override Task<DigitalTwinCommandResponse> OnCommandRequest(DigitalTwinCommandRequest commandRequest)
        {
            Console.WriteLine($"\t Command - {commandRequest.Name} was invoked from the service");
            Console.WriteLine($"\t Data - {commandRequest.Payload}");
            Console.WriteLine($"\t Request Id - {commandRequest.RequestId}.");

            // TODO: trigger the callback and return command response
            return Task.FromResult(new DigitalTwinCommandResponse(200, "{\"payload\": \"data\"}"));
        }

        /// <summary>
        /// Callback on property updated.
        /// </summary>
        /// <param name="propertyUpdate">information regarding the property updated.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected override async Task OnPropertyUpdated(DigitalTwinPropertyUpdate propertyUpdate)
        {
            Console.WriteLine($"Received updates for property '{propertyUpdate.PropertyName}'");

            switch (propertyUpdate.PropertyName)
            {
                case CustomerName:
                    await this.SetCustomerNameAsync(propertyUpdate).ConfigureAwait(false);
                    break;
                case Brightness:
                    await this.SetBrightnessAsync(propertyUpdate).ConfigureAwait(false);
                    break;
                default:
                    Console.WriteLine($"Property name '{propertyUpdate.PropertyName}' is not handled.");
                    break;
            }
        }
    }
}