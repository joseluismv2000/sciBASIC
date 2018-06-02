﻿Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports VisualBasic = Microsoft.VisualBasic.Language.Runtime
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Linq

' author: Kobi Perl
' Based On the following thesis:
'   Eden, E. (2007). Discovering Motifs In Ranked Lists Of DNA Sequences. Haifa. 
'   Retrieved from http://bioinfo.cs.technion.ac.il/people/zohar/thesis/eran.pdf

Module Module1

    Sub Main()

    End Sub

    ''' <summary>
    ''' We define EPSILON to account for small changes in the calculation of p-value
    ''' between the Function calculating the statistic And this Function calculating the p-values
    ''' Specifically, If (statistic + EPSILON) Is higher than the hypergeometric tail associated by a cell
    ''' In the W*B path matrix, used In the p-value calculation, Then the cell Is Not In the "R region"
    ''' We warn ifthe mHG statistic gets below -EPSILON
    ''' </summary>
    Public Const EPSILON# = 0.0000000001

    ''' <summary>
    ''' Performs a minimum-hypergeometric test.
    ''' Test Is based On the following thesis:
    '''   Eden, E. (2007). Discovering Motifs In Ranked Lists Of DNA Sequences. Haifa. 
    '''   Retrieved from http://bioinfo.cs.technion.ac.il/people/zohar/thesis/eran.pdf
    '''
    ''' The null-hypothesis Is that the 1S In the lambda list are randomly And uniformly 
    ''' distributed In the lambdas list. The alternative hypothesis Is that the 1S tend
    ''' To appeard In the top Of the list. As the designation Of "top" Is Not a clear-cut
    ''' multiple hypergeometric tests are performed, With increasing length Of lambdas 
    ''' being considered To be In the "top". The statistic Is the minimal p-value obtained 
    ''' In those tests. A p-value Is calculated based On the statistics.
    ''' </summary>
    ''' <param name="lambdas">``{0,1}^N``, sorted from top to bottom.</param>
    ''' <param name="n_max#">the algorithm will only consider the first n_max partitions.</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' ```R
    ''' mHG.test &lt;- function(lambdas, n_max = length(lambdas)) {...}
    ''' ```
    ''' </remarks>
    Function mHGtest(lambdas As Vector, Optional n_max# = Double.NaN) As htest
        Dim N = lambdas.Length
        Dim B = lambdas.Sum
        Dim W = lambdas.Length - B

        n_max = n_max Or CDbl(lambdas.Length).AsDefault.When(n_max.IsNaNImaginary)

        ' The uncorrected for MHT p-value
        Dim mHGstatisticinfo As mHGstatisticInfo = mHGstatisticcalc(lambdas, n_max)
        Dim p = mHGpvalcalc(mHGstatisticinfo.mHG, N, B, n_max)
        Dim result As New htest With {
            .pvalue = p,
            .n = mHGstatisticinfo.n, ' Not an official field Of htest
            .b = mHGstatisticinfo.b  ' Not an official field Of htest        
        }

        With New VisualBasic
            result.statistic = list(!mHG = mHGstatisticinfo.mHG).AsNumeric
            result.parameters = list(!N = N, !B = B, !n_max = n_max).AsNumeric
        End With

        Return result
    End Function

    Public Function mHGstatisticcalc(lambdas As Vector, Optional n_max# = Double.NaN) As mHGstatisticInfo
        '# Calculates the mHG statistic.
        '# mHG definition:
        '#   mHG(lambdas) = min over 1 <= n < N of HGT (b_n(lambdas); N, B, n)
        '# Where HGT Is the hypergeometric tail:
        '#   HGT(b; N, B, n) = Probability(X >= b)
        '# And:
        '#   b_n = sum over 1 <= i <= n of lambdas[i]
        '# In R, HGT can be obtained using:
        '#   HGT(b; N, B, n) = phyper((b-1), B, N - B, n, lower.tail = F)
        '#
        '# Input:
        '#   lambdas - sorted And labeled {0,1}^N.
        '#   n_max - the algorithm will only consider the first n_max partitions.
        '# Output: mHG.statistic
        '# 
        '# Statistic Is defined in the following thesis:
        '#   Eden, E. (2007). Discovering Motifs in Ranked Lists of DNA Sequences. Haifa. 
        '#   Retrieved from http://bioinfo.cs.technion.ac.il/people/zohar/thesis/eran.pdf
        '# 
        '# If several n gives the same mHG, then the lowest one Is taken.

        n_max = n_max Or CDbl(lambdas.Length).AsDefault.When(n_max.IsNaNImaginary)

        ' Input check
        ' stopifnot(n_max > 0)
        ' stopifnot(n_max <= length(lambdas))
        ' stopifnot(length(lambdas) > 0)
        ' stopifnot(all(lambdas == 1 | lambdas == 0))

        Dim N = lambdas.Length
        Dim B = lambdas.Sum
        Dim W = N - B

        Dim mHG = 1
        Dim mHGn = 0
        Dim mHGb = 0
        Dim m = 0 ' Last time we saw a one
        Dim HG_row As New Vector(B + 1) ' The first B + 1 hypergeometric probabilities, HG[i] = Prob(X == (i - 1))
        Dim HGT As Double

        ' updated For the current number Of tries n.
        HG_row(1) = 1 ' For n = 0, b = 0
        B = 0
        N = 0
        Do While (N < n_max)  ' iterating On different N To find minimal HGT
            N = N + 1
            B = B + lambdas(N)

            If (lambdas(N) = 1.0R) Then  ' Only Then HGT can decrease (see p. 19 In thesis)
                HG_row = HG_row_ncalc(HG_row, m, N, B, N, B)
                m = N

                HGT = 1 - HG_row("1:b").Sum  ' P(X >= b) = 1 - P(X <b)
                ' statistic
                If (HGT < mHG) Then
                    mHG = HGT
                    mHGn = N
                    mHGb = B
                End If
            End If
        Loop
        Return New mHGstatisticInfo With {.mHG = mHG, .n = mHGn, .b = mHGb}
    End Function

    Public Function mHGpvalcalc(p#, N#, B#, Optional n_max# = Double.NaN) As Double
        '# Calculates the p-value associated with the mHG statistic.
        '# Guidelines for the calculation are to be found in:
        '#   Eden, E. (2007). Discovering Motifs in Ranked Lists of DNA Sequences. Haifa. 
        '#   Retrieved from http://bioinfo.cs.technion.ac.il/people/zohar/thesis/eran.pdf
        '# (pages 11-12, 19-20)
        '# Input:
        '#   p - the mHG statistic. Marked as p, as it represenets an "uncorrected" p-value.
        '#   N - total number of white And black balls (according to the hypergeometric problem definition).
        '#   B - number of black balls.
        '#   n_max - the algorithm will calculate the p-value under the null hypothesis that only the 
        '#           first n_max partitions are taken into account in determining in minimum.
        '# Output: p-value.

        n_max = n_max Or N.AsDefault.When(n_max.IsNaNImaginary)

        ' Input check
        'stopifnot(n_max > 0)
        'stopifnot(n_max <= N)
        'stopifnot(N >= B)
        'stopifnot(p <= 1)

        If (p < -EPSILON) Then
            Warning("p-value calculation will be highly inaccurate due to an extremely small mHG statistic")
        End If

        ' p - the statistic.
        ' N\B - number Of all \ black balls.
        Dim W = N - B
        Dim R_separation_line = R_separation_linecalc(p, N, B, n_max)
        Dim pi_r = pi_rcalc(N, B, R_separation_line)
        Dim p_corrected As Double = 1 - pi_r(W + 2, B + 2)

        Return p_corrected
    End Function

    Public Function R_separation_linecalc(p#, N%, B%, n_max%) As Vector
        '# Determine R separation line - This Is the highest (broken) line crossing the B*W matrix horizontally, that underneath it all
        '# the associated p-values are higher than p, Or w + b > n_max.
        '#
        '# (This Is a bit different from the original definition To make the calculation more efficient)
        '#
        '# Input:
        '#   p - the mHG statistic. Marked As p, As it represenets an "uncorrected" p-value.
        '#   N - total number Of white And black balls (according To the hypergeometric problem definition).
        '#   B - number Of black balls.
        '#   n_max - Part Of the constraint On the line, the null hypothesis Is calculated under
        '#           the assumption that the first n_max partitions are taken into account In determining the minimum.
        '# Output:
        '#   R_separation_line - represented As a vector size B + 1, index b + 1 containing 
        '#                       the first (high enough) w To the right Of the R separation line (Or W + 1 If no such w exist).
        '# See:
        '#   Eden, E. (2007). Discovering Motifs In Ranked Lists Of DNA Sequences. Haifa. 
        '#   Retrieved from http://bioinfo.cs.technion.ac.il/people/zohar/thesis/eran.pdf
        '#   (pages 11-12)
        Dim W As Double = N - B
        Dim R_separation_line As Vector = Repeats(W + 1, times:=B + 1)

        Dim HG_row As New Vector(B) ' First B in HG_row
        HG_row(1) = 1 ' For n = 0, b = 0
        B = 0
        W = 0
        Dim HGT = 1 ' Initial HGT

        ' We are tracing the R line - increasing b until we Get To a cell where the associated p-values are smaller
        ' than p, And Then increasing w until we Get To a cell where the associated p-values are bigger than p
        Dim should_inc_w = (HGT <= (p + EPSILON)) AndAlso (W < W) AndAlso (B <= (n_max - W))
        Dim should_inc_b = (HGT > (p + EPSILON)) AndAlso (B < B) AndAlso (B <= (n_max - W))

        Do While (should_inc_w OrElse should_inc_b)
            Do While (should_inc_b)  ' Increase b until we Get To the R line (Or going outside the n_max zone)
                R_separation_line(B + 1) = W
                B = B + 1
                HG_row(B + 1) = HG_row(B) * d_ratio(B + W, B, N, B)
                HG_row("1:B") = HG_row("1:b") * v_ratio(B + W, seq(0, B - 1), N, B)
                HGT = 1 - HG_row("1:B").Sum  ' P(X >= b) = 1 - P(X <b)
                should_inc_b = (HGT > (p + EPSILON)) AndAlso (B < B) AndAlso (B <= (n_max - W))
            Loop
            If (B > (n_max - W)) Then
                ' We can Stop immediately And we Do Not need To calculate HG_row anymore
                R_separation_line("(b+1):(B+1)") = W
                should_inc_w = False
            Else
                should_inc_w = (HGT <= (p + EPSILON)) AndAlso (W < W)
                Do While (should_inc_w) ' Increase w until we Get outside the R line (Or going outside the n_max zone)
                    W = W + 1
                    HG_row("1:(b+1)") = HG_row("1:(b+1)") * v_ratio(B + W, Seq(0, B), N, B)
                    HGT = 1 - HG_row("1:B").Sum  ' P(X >= b) = 1 - P(X <b)
                    should_inc_w = (HGT <= (p + EPSILON)) AndAlso (W < W) AndAlso (B <= (n_max - W))
                Loop
                If (B > (n_max - W)) Then
                    ' We can stop immediately And we do Not need to calculate HG_row anymore
                    R_separation_line("(b+1):(B+1)") = W
                    should_inc_b = False
                Else
                    should_inc_b = (HGT > (p + EPSILON)) AndAlso (B < B) AndAlso (B <= (n_max - W))
                End If
            End If
        Loop
        If (HGT > (p + EPSILON)) Then ' Last one
            R_separation_line(B + 1) = W
        End If
        Return (R_separation_line)
    End Function

    Public Function pi_rcalc(N, B, R_separation_line) As Matrix
        '# Consider an urn With N balls, B Of which are black And W white. pi_r stores 
        '# The probability Of drawing w white And b black balls In n draws (n = w + b)
        '# With the constraint Of P(w,b) = 0 If (w, b) Is On Or above separation line.
        '# Row 1 Of the matrix represents w = -1, Col 1 represents b = -1.
        '#
        '# Input:
        '#   N - total number Of white And black balls (according To the hypergeometric problem definition).
        '#   B - number Of black balls.
        '#   R_separation_line - represented As a vector size B + 1, index b + 1 containing 
        '#                       the first (high enough) w To the right Of the R separation line.
        '# See:
        '#   Eden, E. (2007). Discovering Motifs In Ranked Lists Of DNA Sequences. Haifa. 
        '#   Retrieved from http://bioinfo.cs.technion.ac.il/people/zohar/thesis/eran.pdf
        '#   (pages 20)
        Dim W As Double = N - B

        Dim pi_r As New Matrix(DATA() = 0, nrow = W + 2, ncol = B + 2)
        pi_r(1,) = 0 ' NOTE: Different from the thesis (see page 20 last paragraph),
        ' should be 1 according To that paragraph, but this seems wrong.
        pi_r(, 1) = 0

        For bi As Integer = 0 To B
            Dim wi = R_separation_line(bi + 1)
            Do While (wi < (W + 1))
                If ((wi = 0) AndAlso (bi = 0)) Then
                    pi_r(2, 2) = 1 ' Note, this cell will be 0 If it's left to the R separation line (should not occure) 
                Else
                    ' Apply the recursion rule:
                    ' P(w,b) = P((w,b)|(w-1,b))*P(w-1,b)+P((w,b)|(w,b-1))*P(w,b-1)
                    pi_r(wi + 2, bi + 2) = (W - wi + 1) / (B + W - bi - wi + 1) * pi_r(wi + 1, bi + 2) +
                        (B - bi + 1) / (B + W - bi - wi + 1) * pi_r(wi + 2, bi + 1)
                End If
                wi = wi + 1
            Loop
        Next
        Return (pi_r)
    End Function

    Public Function HG_row_ncalc(HG_row_m, m, ni, b_n, N, B)
        '# Calculate HG row n. This row contains the first (b_n  + 1)
        '# hypergeometric probabilities, HG[i] = Prob(X == (i - 1)), For number Of tries n.
        '# Does so given an updated HG row m (m < n), which contains the first (b_n)
        '# hypergeometric probabilities.
        '#
        '# Input:
        '#   HG_row_m - updated HG row m (m < n), which contains the first (b_n)
        '#              hypergeometric probabilities.
        '#   m - the number Of tries (m < n) For which the HG_row_m fits.
        '#   n - the number Of tries (n > m) For which we want To calculate the HG row
        '#   b_n - The maximal b For which we need To calculate the hypergeometric probabilities.
        '#   N - total number Of white And black balls (according To the hypergeometric problem definition).
        '#   B - number Of black balls.

        '# The Function directs the calculation To an iteration solution (With the cost Of B(n-m)) 
        '# Or a recursive solution (With the cost B * log(B)). This multiplier helps To determine
        '# When To use the recursion solution - it Is Not a theoretical result, but an empirical one.
        Const RECURSION_OVERHEAD_MULTIPLIER = 20

        Dim HG_row_ncalcfunc = Nothing
        If ((N - m) <= (RECURSION_OVERHEAD_MULTIPLIER * Log2(b_n))) Then
            HG_row_ncalcfunc = AddressOf HG_row_ncalciter
        Else
            HG_row_ncalcfunc = AddressOf HG_row_ncalcrecur
        End If

        Return (HG_row_ncalcfunc(HG_row_m, m, ni, b_n, N, B))
    End Function


    Public Function HG_row_ncalciter(HG_row_m As Vector, m#, ni#, b_n#, N#, B#) As Vector
        '# Calculate HG row n iteratively.
        '# See function documentation for "HG_row_n.calc", to gain insight on input And outputs. 

        '# NOTE: The code works directly on HG_row_m, m - increasing m until it becomes n.

        '# Go upwards (increasing only m) until we get to row n-1.    
        Dim b_to_update = seq(0, b_n - 1)

        Do While (m < (N - 1))
            m = m + 1
            HG_row_m(b_to_update + 1) = HG_row_m(b_to_update + 1) * v_ratio(m, b_to_update, N, B)
        Loop

        m = m + 1
        ' Last row To go - first update b_n from the diagonal, Then the rest vertically
        HG_row_m(b_n + 1) = HG_row_m(b_n) * d_ratio(m, b_n, N, B)
        HG_row_m(b_to_update + 1) = HG_row_m(b_to_update + 1) * v_ratio(m, b_to_update, N, B)

        Return (HG_row_m)
    End Function

    Public Function d_ratio(ni, bi, N, B)
        ' The ratio between HG(n,b,B,N) And HG(n-1,b-1,B,N)
        ' See page 19 In Eden's theis.
        Return (N * (B - (B - 1)) / (B * (N - (N - 1))))
    End Function

    Public Function v_ratio(ni, bi, N, B)
        ' The ratio between HG(n,b,B,N) And HG(n-1,b,B,N)
        ' See page 19 In Eden's theis
        Return ((N * (N - N - B + B + 1)) / ((N - B) * (N - N + 1)))
    End Function

End Module

Public Class htest
    Public statistic As Dictionary(Of String, Double)
    Public parameters As Dictionary(Of String, Double)
    Public pvalue As Double
    Public n As Integer
    Public b As Double
End Class

''' <summary>
''' mHG definition:
'''   mHG(lambdas) = min over 1 &lt;= n &lt;= N Of HGT (b_n(lambdas); N, B, n)
''' Where HGT Is the hypergeometric tail:
'''   HGT(b; N, B, n) = Probability(X >= b)
''' And:
'''   b_n = sum over 1 &lt;= i &lt;= n Of lambdas[i]
''' Fields:
'''   mHG - the statistic itself
'''   n - the index For which it was obtained
'''   b (Short For b_n) - sum over 1 &lt;= i &lt;= n Of lambdas[i]
''' </summary>
Class mHGstatisticInfo
    Public mHG As Double
    Public n As Double
    Public b As Double
End Class

