// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.

namespace Msfs.ControllerVisualizer.Tests.Services;

using System.Xml.Linq;
using Msfs.ControllerVisualizer.Models;
using Msfs.ControllerVisualizer.Services;

/// <summary>
/// Unit tests for the <see cref="ControllerButtonMapper"/> class.
/// </summary>
[TestClass]
public class ControllerButtonMapperTests
{
    private ControllerButtonMapper mapper = null!;
    private ControllerDefinition dummyDefinition = null!;

    [TestInitialize]
    public void Setup()
    {
        this.mapper = new();
        this.dummyDefinition = new()
        {
            Name = "Test Controller",
            DeviceName = "Test Device",
            ProductId = "1234",
            VisualFile = "Test.xaml"
        };
    }

    [TestMethod]
    public void MapButtonsExtractsValidMappingsFromXml()
    {
        string xml = """
            <Device DeviceName="Test" ProductID="1234">
                <Context ContextName="AIRCRAFT">
                    <Action ActionName="KEY_AP_MASTER" ValueEvent="0.000000" Delay="0.000000" Flag="2">
                        <Primary>
                            <KEY Information="Joystick Button 20">19</KEY>
                        </Primary>
                    </Action>
                </Context>
            </Device>
            """;
        XElement device = XElement.Parse(xml);

        List<ButtonMapping> mappings = this.mapper.MapButtons(device, this.dummyDefinition);

        Assert.AreEqual(1, mappings.Count);
        Assert.AreEqual("Joystick_Button_20", mappings[0].ButtonId);
        Assert.AreEqual("KEY_AP_MASTER", mappings[0].MsfsCommand);
        Assert.AreEqual("Ap Master", mappings[0].FriendlyName);
    }

    [TestMethod]
    public void MapButtonsReturnsEmptyListWhenNoKeyElements()
    {
        string xml = """
            <Device DeviceName="Test" ProductID="1234">
                <Context ContextName="AIRCRAFT">
                    <Action ActionName="KEY_AP_MASTER" ValueEvent="0.000000" Delay="0.000000" Flag="2"/>
                </Context>
            </Device>
            """;
        XElement device = XElement.Parse(xml);

        List<ButtonMapping> mappings = this.mapper.MapButtons(device, this.dummyDefinition);

        Assert.AreEqual(0, mappings.Count);
    }

    [TestMethod]
    public void MapButtonsSkipsActionsWithEmptyActionName()
    {
        string xml = """
            <Device DeviceName="Test" ProductID="1234">
                <Context ContextName="AIRCRAFT">
                    <Action ActionName="" ValueEvent="0.000000" Delay="0.000000" Flag="2">
                        <Primary>
                            <KEY Information="Joystick Button 1">0</KEY>
                        </Primary>
                    </Action>
                </Context>
            </Device>
            """;
        XElement device = XElement.Parse(xml);

        List<ButtonMapping> mappings = this.mapper.MapButtons(device, this.dummyDefinition);

        Assert.AreEqual(0, mappings.Count);
    }

    [TestMethod]
    public void MapButtonsExtractsMappingsFromMultipleContexts()
    {
        string xml = """
            <Device DeviceName="Test" ProductID="1234">
                <Context ContextName="AIRCRAFT">
                    <Action ActionName="KEY_GEAR_TOGGLE" ValueEvent="0.000000" Delay="0.000000" Flag="2">
                        <Primary>
                            <KEY Information="Joystick Button 1">0</KEY>
                        </Primary>
                    </Action>
                </Context>
                <Context ContextName="INSTRUMENTS_CONTROL">
                    <Action ActionName="KEY_HEADING_BUG_SELECT" ValueEvent="0.000000" Delay="0.000000" Flag="2">
                        <Primary>
                            <KEY Information="Joystick Button 3">2</KEY>
                        </Primary>
                    </Action>
                </Context>
            </Device>
            """;
        XElement device = XElement.Parse(xml);

        List<ButtonMapping> mappings = this.mapper.MapButtons(device, this.dummyDefinition);

        Assert.AreEqual(2, mappings.Count);
        Assert.IsTrue(mappings.Any(m => m.ButtonId == "Joystick_Button_1"));
        Assert.IsTrue(mappings.Any(m => m.ButtonId == "Joystick_Button_3"));
    }

    [TestMethod]
    public void MapButtonsNormalizesIdentifierByReplacingSpacesWithUnderscores()
    {
        string xml = """
            <Device DeviceName="Test" ProductID="1234">
                <Context ContextName="AIRCRAFT">
                    <Action ActionName="KEY_ANTI_ICE_TOGGLE" ValueEvent="0.000000" Delay="0.000000" Flag="2">
                        <Primary>
                            <KEY Information="Joystick Button 24">23</KEY>
                        </Primary>
                    </Action>
                </Context>
            </Device>
            """;
        XElement device = XElement.Parse(xml);

        List<ButtonMapping> mappings = this.mapper.MapButtons(device, this.dummyDefinition);

        Assert.AreEqual("Joystick_Button_24", mappings[0].ButtonId);
    }

    [TestMethod]
    public void MapButtonsTrimsTrailingSpacesFromIdentifier()
    {
        string xml = """
            <Device DeviceName="Test" ProductID="1234">
                <Context ContextName="SURFACES">
                    <Action ActionName="KEY_AXIS_AILERONS_SET" ValueEvent="0.000000" Delay="0.000000" Flag="2">
                        <Primary>
                            <KEY Information="Joystick L-Axis X ">0</KEY>
                        </Primary>
                    </Action>
                </Context>
            </Device>
            """;
        XElement device = XElement.Parse(xml);

        List<ButtonMapping> mappings = this.mapper.MapButtons(device, this.dummyDefinition);

        Assert.AreEqual("Joystick_L-Axis_X", mappings[0].ButtonId);
    }

    [TestMethod]
    public void MapButtonsRemovesKeyPrefixFromFriendlyName()
    {
        string xml = """
            <Device DeviceName="Test" ProductID="1234">
                <Context ContextName="AIRCRAFT">
                    <Action ActionName="KEY_MAGNETO_START" ValueEvent="0.000000" Delay="0.000000" Flag="2">
                        <Primary>
                            <KEY Information="Joystick Button 1">0</KEY>
                        </Primary>
                    </Action>
                </Context>
            </Device>
            """;
        XElement device = XElement.Parse(xml);

        List<ButtonMapping> mappings = this.mapper.MapButtons(device, this.dummyDefinition);

        Assert.AreEqual("Magneto Start", mappings[0].FriendlyName);
    }

    [TestMethod]
    public void MapButtonsCapitalizesEachWordInFriendlyName()
    {
        string xml = """
            <Device DeviceName="Test" ProductID="1234">
                <Context ContextName="AIRCRAFT">
                    <Action ActionName="KEY_TOGGLE_MASTER_BATTERY" ValueEvent="0.000000" Delay="0.000000" Flag="2">
                        <Primary>
                            <KEY Information="Joystick Button 1">0</KEY>
                        </Primary>
                    </Action>
                </Context>
            </Device>
            """;
        XElement device = XElement.Parse(xml);

        List<ButtonMapping> mappings = this.mapper.MapButtons(device, this.dummyDefinition);

        Assert.AreEqual("Toggle Master Battery", mappings[0].FriendlyName);
    }
}
