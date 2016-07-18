﻿Imports System.ComponentModel.DataAnnotations

Public Interface IBufferProvider
    Sub FillAudioBuffer(audioBuffer() As Integer, isFirst As Boolean)
    Sub Close()

    ReadOnly Property Oscillator As Oscillator
    <RangeAttribute(0.0, Double.MaxValue)> Property Frequency As Double
    Property Note As String
    <RangeAttribute(0.0, 1.0)> Property Volume As Double
    ReadOnly Property Envelop As Envelope
    Property Tag As Object
End Interface
