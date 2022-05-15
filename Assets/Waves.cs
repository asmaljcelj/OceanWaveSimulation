#define UNITY

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Waves : MonoBehaviour
{
    public int M = 128;
    public int N = 128;
    public float numericConstant = 1.0f;
    public float Lx = 1.0f;
    public float Lz = 1.0f;
    public float windSpeed = 1.0f;
    public Vector2 w;
    private float g = 9.81f;
    protected MeshFilter meshFilter;
    protected Mesh mesh;
    Complex[] spec2;

    private long time = 0;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        mesh.name = gameObject.name;

        mesh.vertices = GenerateVerts();
        mesh.triangles = GenerateTries();
        mesh.RecalculateBounds();

        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    private Vector3[] GenerateVerts() {
        var verts = new Vector3[(M + 1) * (N + 1)];
        spec2 = new Complex[(M + 1) * (N + 1)];

        for (int n = 0; n <= N; n++) {
            for (int m = 0; m <= M; m++) {
                float x = n * Lx / N;
                float z = m * Lz / M;
                float kx = 2 * Mathf.PI * n / Lx;
                float kz = 2 * Mathf.PI * m / Lz;
                Vector2 k = new Vector2(kx, kz);
                Complex y = getFourierAmplitudesAtTime(k);
                Vector2 position = new Vector2(x, z);
                float dot = Vector2.Dot(k, position);
                float real = Mathf.Cos(dot);
                float im = Mathf.Sin(dot);
                Complex exp = new Complex(real, im);
                Complex result = y * exp;
                float value = result.fMagnitude;
                verts[index(n, m)] = new Vector3(n, value, m);
            }
        }
        return verts;
    }

    public Complex calculateFourierAmplitudes(Vector2 k) {
        float gaussian1 = generateGaussianNum(0, 1);
        float gaussian2 = generateGaussianNum(0, 1);
        float result = 1.0f / Mathf.Sqrt(2.0f);
        float spectrum = calculateWaveSpectrum(k);
        spectrum = Mathf.Sqrt(spectrum);

        result = result * spectrum;
        float real = result * gaussian1;
        float im = result * gaussian2;
        Complex com = new Complex(real, im);

        return com;
    }

    public float calculateWaveSpectrum(Vector2 k) {
        float L = windSpeed * windSpeed / g;
        float kL = k.magnitude * L;
        kL = Mathf.Pow(kL, 2);
        
        float a = Mathf.Exp(-1.0f / kL);
        a = a / Mathf.Pow(k.magnitude, 4);
        a = a * numericConstant;
        Vector2 cosineFactor = Vector2.Scale(k.normalized, w.normalized);
        float value = Mathf.Pow(cosineFactor.magnitude, 2);
        a = a * value;

        return a;
    }

    public Complex getFourierAmplitudesAtTime(Vector2 k) {
        Complex h0 = calculateFourierAmplitudes(k);
        Vector2 kNeg = new Vector2(-k.x, -k.y);
        Complex h0Neg = calculateFourierAmplitudes(kNeg);
        float wk = Mathf.Sqrt(g * k.magnitude);
        float real1 = Mathf.Cos(wk * time);
        float im1 = Mathf.Sin(wk * time);
        Complex iwkt = new Complex(real1, im1);
        float real2 = Mathf.Cos(-wk * time);
        float im2 = Mathf.Sin(-wk * time);
        Complex iwktMinus = new Complex(real2, im2);
        Complex result = h0 * iwkt + h0Neg * iwktMinus;
        return result;
    }

    public int index(int x, int z) {
        return x * (N + 1) + z;
    }


    public float generateGaussianNum(double mean, double stdDev) {
        float u1 = UnityEngine.Random.Range(0f, 1f);
        float u2 = UnityEngine.Random.Range(0f, 1f);
        float temp1 = Mathf.Sqrt(-2 * Mathf.Log(u1));
        float temp2 = 2 * Mathf.PI * u2;

        return (float) (mean + stdDev*(temp1 * System.Math.Cos(temp2)));
    }

    public int[] GenerateTries() {
        var tries = new int[mesh.vertices.Length * 6];

        for (int x = 0; x < M; x++) {
            for (int z = 0; z < N; z++) {
                tries[index(x, z) * 6 + 0] = index(x, z);
                tries[index(x, z) * 6 + 1] = index(x + 1, z + 1);
                tries[index(x, z) * 6 + 2] = index(x + 1, z);
                tries[index(x, z) * 6 + 3] = index(x, z);
                tries[index(x, z) * 6 + 4] = index(x, z + 1);
                tries[index(x, z) * 6 + 5] = index(x + 1, z + 1);
            }
        }
        return tries;
    }

    // Update is called once per frame
    void Update()
    {
        time = time + 1;
        if (time % 500 == 0) {
            mesh.vertices = GenerateVerts();
            mesh.triangles = GenerateTries();
            mesh.RecalculateBounds();
        }
    }
}


public struct Complex
{
    public double real;
    public double img;
    public Complex(double aReal, double aImg)
    {
        real = aReal;
        img = aImg;
    }
    public static Complex FromAngle(double aAngle, double aMagnitude)
    {
        return new Complex(Math.Cos(aAngle) * aMagnitude, Math.Sin(aAngle) * aMagnitude);
    }
    public Complex conjugate { get { return new Complex(real, -img); } }
    public double magnitude { get { return Math.Sqrt(real * real + img * img); } }
    public double sqrMagnitude { get { return real * real + img * img; } }
    public double angle { get { return Math.Atan2(img, real); } }
    public float fReal { get { return (float)real; } set { real = value; } }
    public float fImg { get { return (float)img; } set { img = value; } }
    public float fMagnitude { get { return (float)Math.Sqrt(real * real + img * img); } }
    public float fSqrMagnitude { get { return (float)(real * real + img * img); } }
    public float fAngle { get { return (float)Math.Atan2(img, real); } }
    #region Basic operations + - * /
    public static Complex operator +(Complex a, Complex b)
    {
        return new Complex(a.real + b.real, a.img + b.img);
    }
    public static Complex operator +(Complex a, double b)
    {
        return new Complex(a.real + b, a.img);
    }
    public static Complex operator +(double b, Complex a)
    {
        return new Complex(a.real + b, a.img);
    }
    public static Complex operator -(Complex a)
    {
        return new Complex(-a.real , -a.img);
    }
    public static Complex operator -(Complex a, Complex b)
    {
        return new Complex(a.real - b.real, a.img - b.img);
    }
    public static Complex operator -(Complex a, double b)
    {
        return new Complex(a.real - b, a.img);
    }
    public static Complex operator -(double b, Complex a)
    {
        return new Complex(b- a.real, -a.img);
    }
    public static Complex operator *(Complex a, Complex b)
    {
        return new Complex(a.real * b.real - a.img * b.img, a.real * b.img + a.img * b.real);
    }
    public static Complex operator *(Complex a, double b)
    {
        return new Complex(a.real * b, a.img * b);
    }
    public static Complex operator *(double b, Complex a)
    {
        return new Complex(a.real * b, a.img * b);
    }
    public static Complex operator /(Complex a, Complex b)
    {
        double d = 1d / (b.real * b.real + b.img * b.img);
        return new Complex((a.real * b.real + a.img * b.img) * d, (-a.real * b.img + a.img * b.real) * d);
    }
    public static Complex operator /(Complex a, double b)
    {
        return new Complex(a.real / b, a.img / b);
    }
    public static Complex operator /(double a, Complex b)
    {
        double d = 1d / (b.real * b.real + b.img * b.img);
        return new Complex(a * b.real*d, -a * b.img);
    }
    #endregion Basic operations + - * /
    #region Trigonometic operations
    public static Complex Sin(Complex a)
    {
        return new Complex(Math.Sin(a.real)*Math.Cosh(a.img), Math.Cos(a.real) * Math.Sinh(a.img));
    }
    public static Complex Cos(Complex a)
    {
        return new Complex(Math.Cos(a.real) * Math.Cosh(a.img), -Math.Sin(a.real) * Math.Sinh(a.img));
    }
    private static double arcosh(double a)
    {
        return Math.Log(a + Math.Sqrt(a*a-1));
    }
    private static double artanh(double a)
    {
        return 0.5 * Math.Log((1+a) / (1-a));
    }
    public static Complex ASin(Complex a)
    {
        double r2 = a.real * a.real;
        double i2 = a.img * a.img;
        double sMag = r2 + i2;
        double c = Math.Sqrt((sMag-1)*(sMag-1) + 4 * i2);
        double sr = a.real > 0 ? 0.5 : a.real < 0 ? -0.5 : 0;
        double si = a.img > 0 ? 0.5 : a.img < 0 ? -0.5 : 0;
        return new Complex(sr*Math.Acos(c - sMag),si*arcosh(c+sMag));
    }
    public static Complex ACos(Complex a)
    {
        return Math.PI * 0.5 - ASin(a);
    }
    public static Complex Sinh(Complex a)
    {
        return new Complex(Math.Sinh(a.real) * Math.Cos(a.img), Math.Cosh(a.real) * Math.Sin(a.img));
    }
    public static Complex Cosh(Complex a)
    {
        return new Complex(Math.Cosh(a.real) * Math.Cos(a.img), Math.Sinh(a.real) * Math.Sin(a.img));
    }
    public static Complex Tan(Complex a)
    {
        return new Complex(Math.Sin(2*a.real)/(Math.Cos(2*a.real)+Math.Cosh(2*a.img)), Math.Sinh(2*a.img)/(Math.Cos(2*a.real) + Math.Cosh(2*a.img)));
    }
    public static Complex ATan(Complex a)
    {
        double sMag = a.real * a.real + a.img * a.img;
        double i = 0.5* artanh(2*a.img / (sMag + 1));
        if (a.real == 0)
            return new Complex(a.img > 1 ? Math.PI * 0.5 : a.img < -1 ? -Math.PI * 0.5 : 0, i);
        double sr = a.real > 0 ? 0.5 : a.real < 0 ? -0.5 : 0;
        return new Complex(0.5 * (Math.Atan((sMag - 1) / (2 * a.real)) + Math.PI * sr), i);
    }
    #endregion Trigonometic operations
    public static Complex Exp(Complex a)
    {
        double e = Math.Exp(a.real);
        return new Complex(e * Math.Cos(a.img), e * Math.Sin(a.img));
    }
    public static Complex Log(Complex a)
    {
        return new Complex(Math.Log(Math.Sqrt(a.real*a.real + a.img*a.img)), Math.Atan2(a.img, a.real));
    }
    public Complex sqrt(Complex a)
    {
        double r = Math.Sqrt(Math.Sqrt(a.real * a.real + a.img * a.img));
        double halfAngle = 0.5 * Math.Atan2(a.img, a.real);
        return new Complex(r * Math.Cos(halfAngle), r * Math.Sin(halfAngle));
    }

    #if UNITY
            public static explicit operator UnityEngine.Vector2(Complex a)
            {
                return new UnityEngine.Vector2((float)a.real, (float)a.img);
            }
            public static explicit operator UnityEngine.Vector3(Complex a)
            {
                return new UnityEngine.Vector3((float)a.real, (float)a.img);
            }
            public static explicit operator UnityEngine.Vector4(Complex a)
            {
                return new UnityEngine.Vector4((float)a.real, (float)a.img);
            }
            public static implicit operator Complex(UnityEngine.Vector2 a)
            {
                return new Complex(a.x, a.y);
            }
            public static implicit operator Complex(UnityEngine.Vector3 a)
            {
                return new Complex(a.x, a.y);
            }
            public static implicit operator Complex(UnityEngine.Vector4 a)
            {
                return new Complex(a.x, a.y);
            }
    #endif
}
