#include "pch.h"

#include "peakLimiter.h"

// C style export to allow access from C# class

extern "C" 
{
    __declspec(dllexport) void * CreateLimiter(
        float fAttck,
        float fRelease,
        float fThreshold,
        int iChannels,
        int iSampleRate
    ) 
    {
        return (void *) new PeakLimiter(fAttck, fRelease, fThreshold, iChannels, iSampleRate);
    }
    __declspec(dllexport) void ReleaseLimiter(void * limiter)
    {
        if (limiter != NULL) 
            delete ((PeakLimiter*)limiter);
    }


    __declspec(dllexport) int applyLimiter_E_I(void* limiter, float* samples, int count)
    {
        if (limiter != NULL)
        {
            PeakLimiter* poLimiter = (PeakLimiter*)limiter;
            return poLimiter->applyLimiter_E_I(samples, count);
        }
        else
        {
            return 0;
        }
    }

    __declspec(dllexport) int applyLimiter(void* limiter, const float** samplesIn, float** samplesOut, int count)
    {
        if (limiter != NULL)
        {
            PeakLimiter* poLimiter = (PeakLimiter*)limiter;
            return poLimiter->applyLimiter(samplesIn, samplesOut, count);
        }
        else
        {
            return 0;
        }
    }

    __declspec(dllexport) int applyLimiter_E(void* limiter, const float* samplesIn, float* samplesOut, int count)
    {
        if (limiter != NULL)
        {
            PeakLimiter* poLimiter = (PeakLimiter*)limiter;
            return poLimiter->applyLimiter_E(samplesIn,samplesOut, count);
        }
        else
        {
            return 0;
        }
    }

    __declspec(dllexport) int applyLimiter_I(void* limiter, float** samples, int count)
    {
        if (limiter != NULL)
        {
            PeakLimiter* poLimiter = (PeakLimiter*)limiter;
            return poLimiter->applyLimiter_I(samples, count);
        }
        else
        {
            return 0;
        }
    }

    __declspec(dllexport) float getMaxGainReduction(void* limiter)
    {
        if (limiter != NULL)
        {
            PeakLimiter* poLimiter = (PeakLimiter*)limiter;
            return poLimiter->getLimiterMaxGainReduction();
        }
        else
        {
            return 0.0f;
        }
    }

    //
    __declspec(dllexport) float getAttack(void* limiter)
    {
        if (limiter != NULL)
        {
            PeakLimiter* poLimiter = (PeakLimiter*)limiter;
            return poLimiter->getLimiterAttack();
        }
        else
        {
            return 0.0f;
        }
    }

    __declspec(dllexport) float getRelease(void* limiter)
    {
        if (limiter != NULL)
        {
            PeakLimiter* poLimiter = (PeakLimiter*)limiter;
            return poLimiter->getLimiterRelease();
        }
        else
        {
            return 0.0f;
        }
    }

    __declspec(dllexport) float getThreshold(void* limiter)
    {
        if (limiter != NULL)
        {
            PeakLimiter* poLimiter = (PeakLimiter*)limiter;
            return poLimiter->getLimiterThreshold();
        }
        else
        {
            return 0.0f;
        }
    }

    __declspec(dllexport) int getDelay(void* limiter)
    {
        if (limiter != NULL)
        {
            PeakLimiter* poLimiter = (PeakLimiter*)limiter;
            return poLimiter->getLimiterDelay();
        }
        else
        {
            return 0;
        }
    }

    __declspec(dllexport) int getSampleRate(void* limiter)
    {
        if (limiter != NULL)
        {
            PeakLimiter* poLimiter = (PeakLimiter*)limiter;
            return poLimiter->getLimiterSampleRate();
        }
        else
        {
            return 0;
        }
    }

    __declspec(dllexport) int setAttack(void* limiter, float value)
    {
        if (limiter != NULL)
        {
            PeakLimiter* poLimiter = (PeakLimiter*)limiter;
            return poLimiter->setLimiterAttack(value);
        }
        else
        {
            return 0;
        }
    }

    __declspec(dllexport) int setRelease(void* limiter, float value)
    {
        if (limiter != NULL)
        {
            PeakLimiter* poLimiter = (PeakLimiter*)limiter;
            return poLimiter->setLimiterRelease(value);
        }
        else
        {
            return 0;
        }
    }

    __declspec(dllexport) int setThreshold(void* limiter, float value)
    {
        if (limiter != NULL)
        {
            PeakLimiter* poLimiter = (PeakLimiter*)limiter;
            return poLimiter->setLimiterThreshold(value);
        }
        else
        {
            return 0;
        }
    }

    __declspec(dllexport) int setSampleRate(void* limiter, int value)
    {
        if (limiter != NULL)
        {
            PeakLimiter* poLimiter = (PeakLimiter*)limiter;
            return poLimiter->setLimiterSampleRate(value);
        }
        else
        {
            return 0;
        }
    }

    __declspec(dllexport) int setNChannels(void* limiter, int value)
    {
        if (limiter != NULL)
        {
            PeakLimiter* poLimiter = (PeakLimiter*)limiter;
            return poLimiter->setLimiterNChannels(value);
        }
        else
        {
            return 0;
        }
    }

}