// **********************************************************************
//
// Copyright (c) 2003-2018 ZeroC, Inc. All rights reserved.
//
// This copy of Ice is licensed to you under the terms described in
// the ICE_LICENSE file included in this distribution.
//
// **********************************************************************

#import <objc/Ice/SlicedData.h>

#include <Ice/SlicedData.h>

@interface ICESlicedData : NSObject<ICESlicedData>
{
@private
    Ice::SlicedData* slicedData_;
}
-(id) initWithSlicedData:(Ice::SlicedData*)slicedData;
-(Ice::SlicedData*) slicedData;
@end
