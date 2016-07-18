﻿Imports System.ComponentModel.DataAnnotations
Imports System.Threading

''' <summary>
''' Represents the limiting curvature that should be applied to the signal generated by an <see cref="Oscillator"/>. 
''' </summary>
Public Class Envelope
    Implements IDisposable

    ''' <summary>
    ''' Structure that represents when a limiting value change should be applied.
    ''' </summary>
    Public Class EnvelopePoint
        ''' <summary>
        ''' Gets or sets the attenuation value.
        ''' </summary>
        ''' <returns><see cref="Double"/></returns>
        <RangeAttribute(0.0, 1.0)> Public Property Volume As Double ' 0 - 1
        ''' <summary>
        ''' Gets or sets the time (in milliseconds) when to apply the <see cref="Volume"/>. 
        ''' </summary>
        ''' <returns><see cref="Integer"/></returns>
        <RangeAttribute(0, Integer.MaxValue)> Public Property Duration As Integer ' milliseconds

        Public Sub New(volume As Double, duration As Integer)
            Me.Volume = volume
            Me.Duration = duration
        End Sub
    End Class

    Public Enum EnvelopeSteps
        Idle = 0
        Attack = 1
        Decay = 2
        Sustain = 3
        Release = 4
    End Enum

    Private mVolume As Double

    Public Property Attack As EnvelopePoint
    Public Property Decay As EnvelopePoint
    Public Property Sustain As EnvelopePoint
    Public Property Release As EnvelopePoint

    Private lastVolume As Double
    Private mEnvStep As EnvelopeSteps
    Private mainThread As Thread
    Private counter As Integer
    Private abortThreads As Boolean

    ''' <summary>
    ''' This event is triggered when a new <see cref="EnvelopePoint"/> is applied. 
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Public Event EnvelopStepChanged(sender As Object, e As EventArgs)

    Public Sub New()
        mEnvStep = EnvelopeSteps.Idle

        Me.Attack = New EnvelopePoint(1, 1)
        Me.Decay = New EnvelopePoint(1, 1)
        Me.Sustain = New EnvelopePoint(1, Integer.MaxValue)
        Me.Release = New EnvelopePoint(0, 1)

        mainThread = New Thread(AddressOf MainLoop)
        mainThread.Start()
    End Sub

    Public Sub New(attack As EnvelopePoint, decay As EnvelopePoint, sustain As EnvelopePoint, release As EnvelopePoint)
        Me.New()

        Me.Attack = attack
        Me.Decay = decay
        Me.Release = release
    End Sub

    ''' <summary>
    ''' Gets the current attenuation value of the envelop, based on its current <see cref="EnvelopePoint"/>  
    ''' </summary>
    ''' <returns><see cref="Double"/> </returns>
    <RangeAttribute(0.0, 1.0)>
    Public ReadOnly Property Volume As Double
        Get
            Return mVolume
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the type of <see cref="EnvelopePoint"/> being applied. 
    ''' </summary>
    ''' <returns><see cref="EnvelopeSteps"/></returns>
    Public Property EnvelopStep As EnvelopeSteps
        Get
            Return mEnvStep
        End Get
        Protected Set(value As EnvelopeSteps)
            mEnvStep = value
            counter = 0
            lastVolume = Volume

            RaiseEvent EnvelopStepChanged(Me, New EventArgs())
        End Set
    End Property

    ''' <summary>
    ''' Begins the envelop processing, starting with the <see cref="EnvelopePoint"/> defined as <see cref="EnvelopeSteps.Attack"/>  
    ''' </summary>
    Public Sub Start()
        mVolume = 0
        EnvelopStep = EnvelopeSteps.Attack
    End Sub

    ''' <summary>
    ''' Triggers the envelop processing for the <see cref="EnvelopePoint"/> defined as <see cref="EnvelopeSteps.Release"/>  
    ''' </summary>
    Public Sub [Stop]()
        EnvelopStep = EnvelopeSteps.Release
    End Sub

    Private Sub MainLoop()
        Dim ep As New EnvelopePoint(0, 1)

        Do
            Thread.Sleep(1)

            If mEnvStep <> EnvelopeSteps.Idle Then
                counter += 1
                Select Case mEnvStep
                    Case EnvelopeSteps.Attack : ep = Attack
                    Case EnvelopeSteps.Decay : ep = Decay
                    Case EnvelopeSteps.Sustain : ep = Sustain
                    Case EnvelopeSteps.Release : ep = Release
                End Select

                mVolume = (ep.Duration - counter) / ep.Duration * lastVolume + counter / ep.Duration * ep.Volume
                If counter >= ep.Duration Then
                    Select Case mEnvStep
                        Case EnvelopeSteps.Attack : EnvelopStep = EnvelopeSteps.Decay
                        Case EnvelopeSteps.Decay : EnvelopStep = EnvelopeSteps.Sustain
                        Case EnvelopeSteps.Sustain : EnvelopStep = EnvelopeSteps.Release
                        Case EnvelopeSteps.Release : EnvelopStep = EnvelopeSteps.Idle
                    End Select
                End If
            End If
        Loop Until abortThreads
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                abortThreads = True
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        ' TODO: uncomment the following line if Finalize() is overridden above.
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class
