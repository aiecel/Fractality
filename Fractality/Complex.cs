using System;

namespace Fractality
{
    public struct Complex
    {
        private double real;
        private double imaginary;

        public double Real
        {
            get => real;
            set => real = value;
        }

        public double Imaginary
        {
            get => imaginary;
            set => imaginary = value;
        }

        public Complex(double real, double imaginary)
        {
            this.real = real;
            this.imaginary = imaginary;
        }

        public static Complex operator + (Complex c1, Complex c2)
        {
            return new Complex(c1.Real + c2.Real, c1.Imaginary + c2.Imaginary);
        }
        
        public Complex Square()
        {
            return new Complex(Real * Real - Imaginary * Imaginary, 2 * Imaginary * Real);
        }
        
        public double Mod()
        {
            return Math.Sqrt(Real * Real + Imaginary * Imaginary);
        }
    }
}