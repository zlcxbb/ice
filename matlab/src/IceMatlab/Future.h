// **********************************************************************
//
// Copyright (c) 2003-2017 ZeroC, Inc. All rights reserved.
//
// This copy of Ice is licensed to you under the terms described in the
// ICE_LICENSE file included in this distribution.
//
// **********************************************************************

#include <Ice/Ice.h>

namespace IceMatlab
{

class Future
{
public:

    void token(std::function<void()>);
    bool waitUntilFinished();

    virtual void exception(std::exception_ptr);
    std::exception_ptr getException() const;

    virtual void sent();
    virtual std::string state() const = 0;
    void cancel();

protected:

    virtual bool isFinished() const = 0;

    std::mutex _mutex;
    std::condition_variable _cond;
    using Lock = std::unique_lock<std::mutex>;

    std::function<void()> _token;
    std::exception_ptr _exception; // If a local exception occurs.
};

//
// For invocations that are considered completed when sent.
//
class SentFuture : public Future
{
public:

    SentFuture();

    virtual void sent();
    virtual std::string state() const;

protected:

    virtual bool isFinished() const;

private:

    bool _sent;
};

class SimpleFuture : public Future
{
public:

    SimpleFuture();

    void done();

    virtual std::string state() const;

protected:

    virtual bool isFinished() const;

private:

    bool _done;
};

}
